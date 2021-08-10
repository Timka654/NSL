using SocketCore.Utils;
using System;

namespace SCL.Unity
{
    public class ClientOptions<T> : SCL.ClientOptions<T> where T : BaseSocketNetworkClient
    {
        public override void RunClientConnect()
        {
            ThreadHelper.InvokeOnMain(() =>
            {
                base.RunClientConnect();
            });
        }

        protected override void OnRunClientDisconnect()
        {
            ThreadHelper.InvokeOnMain(() =>
            {
                base.OnRunClientDisconnect();
            });
        }
        public override void RunException(Exception ex)
        {
            ThreadHelper.InvokeOnMain(() =>
            {
                base.RunException(ex);
            });
        }
    }
}
