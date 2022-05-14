using System;

namespace NSL.SocketClient.Unity
{
    public class UnityClientOptions<T> : SocketClient.ClientOptions<T> where T : BaseSocketNetworkClient
    {
        public override void RunClientConnect()
        {
            ThreadHelper.InvokeOnMain(() => base.RunClientConnect());
        }

        protected override void OnRunClientDisconnect()
        {
            ThreadHelper.InvokeOnMain(() => base.OnRunClientDisconnect());
        }

        public override void RunException(Exception ex)
        {   
            ThreadHelper.InvokeOnMain(() => base.RunException(ex));
        }
    }
}
