#if NSL_UTILS

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NSL.Utils
{
    public class DeferredAsyncAction : IDisposable
    {
        CancellationTokenSource cts = new CancellationTokenSource();

        CancellationTokenSource ects;

        public DeferredAsyncAction(Func<CancellationToken, Task> action) : this(async (c) => { await action(c); return true; })
        {
        }

        public DeferredAsyncAction(Func<CancellationToken, Task<bool>> action)
        {
            this.action = action;
            ects = cts;
        }

        public void AddCancellationToken(CancellationToken c)
            => ects = CancellationTokenSource.CreateLinkedTokenSource(ects.Token, c);

        public CancellationToken CancellationToken => ects.Token;

        private Func<CancellationToken, Task<bool>> action;

        public bool Started { get; private set; }

        public bool Finished { get; private set; }

        public async Task Invoke()
        {
            if (CancellationToken.IsCancellationRequested)
                return;

            Started = true;

            try
            {
                Finished = await action(CancellationToken);
            }
            catch (OperationCanceledException)
            {
            }

            Dispose();
        }

        public async Task<bool> WaitForExecution(TimeSpan? delay = null, bool disposeExpired = false)
        {
            try
            {
                await Task.Delay(delay ?? Timeout.InfiniteTimeSpan, CancellationToken);
            }
            catch (OperationCanceledException)
            { }
            catch (ObjectDisposedException)
            { }

            if (disposeExpired)
                Dispose();

            return Finished;
        }

        public void Dispose()
        {
            try
            {
                if (!cts.Token.IsCancellationRequested)
                    cts.Cancel();

                cts.Dispose();

            }
            catch (Exception)
            {
            }
        }
    }
}


#endif