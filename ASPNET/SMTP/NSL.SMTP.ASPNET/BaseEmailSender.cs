using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using NSL.DataSource.ASPNET;
using NSL.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NSL.SMTP.ASPNET
{
    public abstract class BaseEmailSender(IOptions<SMTPConfigurationModel> options, ILogger logger, IServiceProvider serviceProvider) : IEmailSender, IHostedService
    {
        static SMTPByteArrayComparer trapComparer = new SMTPByteArrayComparer();

        protected SmtpStatusCode[] UserInputRelatedCodes { get; } =
        [
            SmtpStatusCode.MailboxUnavailable,
            SmtpStatusCode.MailboxNameNotAllowed,
            SmtpStatusCode.SyntaxError
        ];

        protected SMTPConfigurationModel Options => options.Value;

        protected virtual string StoreDataName { get; } = "email_sender.data";

        ConcurrentDictionary<byte[], DateTime> trapCollection = new(trapComparer);
        DateTime lastTrapClear = DateTime.UtcNow;

        protected record SendMailRequest(string email, string subject, string htmlMessage, Guid? id, string uid, byte[] hash, TimeSpan? delay);

        DateTime now = DateTime.UtcNow;

        protected void TryClearTrap()
            => trapCollection.Clear();

        protected bool TryRemoveTrap(byte[] hash)
            => trapCollection.Remove(hash, out _);

        public byte[] GenerateTrapHash(string email
            , string subject
            , string uid)
            => SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("__", subject, email, uid)));

        public async Task<bool> SendEmailAsync(string email
            , string subject
            , string htmlMessage
            , string uid
            , TimeSpan? delay = null
            , byte[] hash = default)
        {
            if (Options.Enabled)
            {
                if (delay.HasValue)
                {
                    hash ??= GenerateTrapHash(email, subject, uid);

                    var trapped = trapCollection.TryGetValue(hash, out var trapDate) && now - trapDate < delay.Value;

                    trapCollection[hash] = now;

                    if (trapped)
                        return false;
                }

                await AddAction(new SendMailRequest(
                    email,
                    subject,
                    htmlMessage,
                    default,
                    uid,
                    hash,
                    delay), CancellationToken.None);

                return true;
            }

            return false;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (Options.Enabled)
                await AddAction(new SendMailRequest(email, subject, htmlMessage, default, default, default, default), CancellationToken.None);
        }

        protected ValueTask AddAction(SendMailRequest data, CancellationToken cancellationToken)
        => channel.AddAsync(new DeferredSendMailAction(t => channelProcessing(data, t), data), cancellationToken);

        DeferredChannel<DeferredSendMailAction> channel = new DeferredChannel<DeferredSendMailAction>();


        async Task channelProcessing(SendMailRequest current, CancellationToken cancellationToken)
        {
            now = DateTime.UtcNow;

            if ((now - lastTrapClear).TotalHours > 3)
            {
                lastTrapClear = now;
                var trapItems = trapCollection.ToArray();

                foreach (var trapItem in trapItems)
                {
                    if ((now - trapItem.Value).TotalHours > 6)
                    {
                        trapCollection.TryRemove(trapItem.Key, out _);
                    }
                }
            }

            await Task.Delay(Options.IterDelayMSeconds, cancellationToken);

            using var cts_src = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));

            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
                    cts_src.Token,
                    cancellationToken
                );

            using var client = await ConfigureClient(current);

            using var msg = await BuildMessageAsync(client, current, cancellationToken);

            msg.IsBodyHtml = true;

            try
            {
                using var scts_src = new CancellationTokenSource(TimeSpan.FromSeconds(Options.RequestDelaySeconds));
                using var scts = CancellationTokenSource.CreateLinkedTokenSource(
                    scts_src.Token,
                    cancellationToken);

                await client.SendMailAsync(msg, scts.Token);
            }
            catch (Exception ex)
            {
                await ExceptionHandleAsync(ex, current, cancellationToken);

                logger.LogError($"{current.email}, {current.subject} - {ex.ToString()}");

                if (await RepeatConditionAsync(ex, current, cancellationToken))
                    await AddAction(current, cancellationToken);

                if (await ThrowDelayConditionAsync(ex, current, cancellationToken))
                    await Task.Delay(Options.ServerThrowDelayMSeconds, cancellationToken);
            }
        }

        protected virtual Task<MailMessage> BuildMessageAsync(SmtpClient client, SendMailRequest mail, CancellationToken cancellationToken)
            => Task.FromResult(new MailMessage(Options.DisplayName ?? Options.UserName, mail.email, mail.subject, mail.htmlMessage));

        protected virtual async Task<SmtpClient> ConfigureClient(SendMailRequest mail)
        {
            return new SmtpClient(Options.Host, Options.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Options.UserName, Options.Password),
                EnableSsl = Options.EnableSsl
            };
        }

        protected virtual Task ExceptionHandleAsync(Exception? ex, SendMailRequest mail, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Condition for check return message can-be return to collection for try send send again
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="mail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task<bool> RepeatConditionAsync(Exception? ex, SendMailRequest mail, CancellationToken cancellationToken)
        {
            if (ex != null && ex is SmtpException smtpEx)
                return !UserInputRelatedCodes.Contains(smtpEx.StatusCode);

            return true;
        }

        /// <summary>
        /// Condition for check 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="mail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task<bool> ThrowDelayConditionAsync(Exception? ex, SendMailRequest mail, CancellationToken cancellationToken)
        {
            return (ex is SmtpException || ex is OperationCanceledException) && !cancellationToken.IsCancellationRequested;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var restoreDataSource = TryGetRestoreDataSource();

            if (restoreDataSource != null)
            {
                var items = await restoreDataSource.TryGetDataAsync<SendMailRequest[]>(StoreDataName, cancellationToken);

                if (items != default)
                {
                    foreach (var item in items)
                    {
                        await AddAction(item, cancellationToken);
                    }
                }
            }

            channel.RunProcessing();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await channel.DisposeAsync();

            var restoreDataSource = TryGetRestoreDataSource();

            if (restoreDataSource == null) return;

            var items = new List<SendMailRequest>();

            await foreach (var item in channel.Channel.Reader.ReadAllAsync())
            {
                items.Add(item.Data);
            }

            if (items.Any())
                await restoreDataSource.SetDataAsync(StoreDataName, items, cancellationToken);
            else
                await restoreDataSource.RemoveDataAsync(StoreDataName, cancellationToken);
        }

        protected virtual IRestoreDataSource? TryGetRestoreDataSource()
            => serviceProvider.GetService<IRestoreDataSource>();

        class DeferredSendMailAction : DeferredAsyncAction
        {
            public DeferredSendMailAction(Func<CancellationToken, Task> action, BaseEmailSender.SendMailRequest data) : base(action)
            {
                Data = data;
            }

            public BaseEmailSender.SendMailRequest Data { get; }
        }
    }
}
