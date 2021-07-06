using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCL.Unity
{
    public class Client<T> : SCL.Client<T> where T : BaseSocketNetworkClient
    {
        public Client(SCL.ClientOptions<T> options) : base(options)
        {
        }

        protected override void OnReceive(ushort pid, int len)
        {
            ThreadHelper.InvokeOnMain(()=>base.OnReceive(pid, len));
        }
        protected override void OnSend(OutputPacketBuffer rbuff, string stackTrace = "")
        {
            ThreadHelper.InvokeOnMain(() => base.OnSend(rbuff, stackTrace));
        }
    }
}
