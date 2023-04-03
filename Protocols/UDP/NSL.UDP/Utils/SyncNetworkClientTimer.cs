using System;
using System.Threading.Tasks;

namespace NSL.UDP.Utils
{
    internal class SyncNetworkClientTimer
    {
        public static event Action OnSync = () => { };

        static SyncNetworkClientTimer()
        {
            RunTimer();
        }

        static async void RunTimer()
        {
            while (true)
            {
                OnSync();

                await Task.Delay(1000);
            }
        }
    }
}
