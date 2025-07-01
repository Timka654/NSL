#if NSL_UTILS

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NSL.Utils
{
    /// <summary>
    /// Channel wrapper for safe processing deferred async actions.
    /// </summary>
    /// <typeparam name="TDef"></typeparam>
    public class DeferredChannel<TDef> : IAsyncDisposable
        where TDef : DeferredAsyncAction
    {
        public CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();

        public Channel<TDef> Channel { get; }

        protected virtual Channel<TDef> CreateChannel()
            => System.Threading.Channels.Channel.CreateUnbounded<TDef>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });

        public DeferredChannel()
        {
            Channel = CreateChannel();
        }

        public ValueTask AddAsync(TDef defAction, CancellationToken token)
            => Channel.Writer.WriteAsync(defAction, token);

        public DeferredChannel<TDef> RunProcessing()
        {
            if (processTask == null)
            {
                processTask = Process();
            }

            return this;
        }

        private Task processTask;

        private async Task Process()
        {
            try
            {
                var reader = Channel.Reader;
                var token = TokenSource.Token;

                while (!token.IsCancellationRequested)
                {
                    var i = await reader.ReadAsync(token).DefaultIfCancelled();

                    if (i == default)
                        continue;

                    do
                    {
                        i.AddCancellationToken(token);

                        await i.Invoke();
                    } while (reader.TryRead(out i) && !token.IsCancellationRequested);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async ValueTask DisposeAsync()
        {
            TokenSource.Cancel();

            if (processTask != null)
                await processTask;
        }
    }
}


#endif