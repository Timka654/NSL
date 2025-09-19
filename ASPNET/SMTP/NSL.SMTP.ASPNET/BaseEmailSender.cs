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
    public abstract class BaseEmailSender : IEmailSender, IHostedService
    {
        public abstract Task SendEmailAsync(string email, string subject, string htmlMessage);

        public abstract Task StartAsync(CancellationToken cancellationToken);

        public abstract Task StopAsync(CancellationToken cancellationToken);
    }

    public abstract class BaseEmailSender<TData>(IOptions<SMTPConfigurationModel> options, ILogger logger, IServiceProvider serviceProvider) : BaseEmailSender
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

        protected record SendMailRequest(string email, string subject, string htmlMessage, Guid? id, string uid, byte[] hash, TimeSpan? delay, TData data);

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
            , byte[] hash = default
            , TData data = default(TData))
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
                    delay,
                    data), CancellationToken.None);

                return true;
            }

            return false;
        }

        public override async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (Options.Enabled)
                await AddAction(new SendMailRequest(email, subject, htmlMessage, default, default, default, default, default), CancellationToken.None);
        }

        protected ValueTask AddAction(SendMailRequest data, CancellationToken cancellationToken)
        => channel.AddAsync(new DeferredSendMailAction<TData>(t => channelProcessing(data, t), data), cancellationToken);

        DeferredChannel<DeferredSendMailAction<TData>> channel = new DeferredChannel<DeferredSendMailAction<TData>>();


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

            try
            {
                using var delayRequestTS = new CancellationTokenSource(TimeSpan.FromSeconds(Options.RequestDelaySeconds));

                using var scts = CancellationTokenSource.CreateLinkedTokenSource(
                    delayRequestTS.Token,
                    cancellationToken);

                using var client = await ConfigureClient(current, cancellationToken);

                using var msg = await BuildMessageAsync(client, current, cancellationToken);

                msg.IsBodyHtml = true;

                await client.SendMailAsync(msg, scts.Token);

                if (haveStored && channel.Channel.Reader.Count == 0)
                {
                    haveStored = false;

                    await restoreDataSource.RemoveDataAsync(StoreDataName, cancellationToken);
                }
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

        protected virtual async Task<SmtpClient> ConfigureClient(SendMailRequest mail, CancellationToken cancellationToken)
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

        bool haveStored = false;
        IRestoreDataSource? restoreDataSource = default;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            restoreDataSource = TryGetRestoreDataSource();

            if (restoreDataSource != null)
            {
                var items = await restoreDataSource.TryGetDataAsync<SendMailRequest[]>(StoreDataName, cancellationToken);

                if (items != default)
                {
                    foreach (var item in items)
                    {
                        haveStored = true;

                        await AddAction(item, cancellationToken);
                    }
                }
            }

            channel.RunProcessing();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await channel.DisposeAsync(true);

            if (restoreDataSource == null) return;

            var items = new List<SendMailRequest>();

            while (channel.Channel.Reader.TryRead(out var item))
            {
                items.Add(item.Data);
            }

            if (items.Any())
            {
                await restoreDataSource.SetDataAsync(StoreDataName, items, cancellationToken);
            }
            else
            {
                await restoreDataSource.RemoveDataAsync(StoreDataName, cancellationToken);
            }
        }

        protected virtual IRestoreDataSource? TryGetRestoreDataSource()
            => serviceProvider.GetService<IRestoreDataSource>();

        class DeferredSendMailAction<TData> : DeferredAsyncAction
        {
            public DeferredSendMailAction(Func<CancellationToken, Task> action, BaseEmailSender<TData>.SendMailRequest data) : base(action)
            {
                Data = data;
            }

            public BaseEmailSender<TData>.SendMailRequest Data { get; }
        }
    }
}
