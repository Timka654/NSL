using System;
using System.Collections.Generic;
using System.Text;

namespace SCL.Unity
{
    public class ClientOptions<T> : SCL.ClientOptions<T> where T : BaseSocketNetworkClient
    {
        protected override void OnRunClientConnect()
        {
            ThreadHelper.InvokeOnMain(() =>
            {
                base.OnRunClientConnect();
            });
        }

        protected override void OnRunClientDisconnect()
        {
            ThreadHelper.InvokeOnMain(() =>
            {
                base.OnRunClientDisconnect();
            });
        }

        protected override void OnRunException(Exception ex)
        {
            ThreadHelper.InvokeOnMain(() =>
            {
                base.OnRunException(ex);
            });
        }
    }
}
