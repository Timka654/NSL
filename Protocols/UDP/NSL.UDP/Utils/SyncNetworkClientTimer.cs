using System;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.UDP.Utils
{
    internal class SyncNetworkClientTimer
    {
        public static event Action OnSync = () => { };

        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();

        static SyncNetworkClientTimer()
        {
            RunTimer(_cts.Token);
        }

        public static void Shutdown() => _cts.Cancel();

        static async void RunTimer(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                OnSync();

                try { await Task.Delay(1000, cancellationToken); }
                catch (TaskCanceledException) { break; }
            }
        }
    }
}
