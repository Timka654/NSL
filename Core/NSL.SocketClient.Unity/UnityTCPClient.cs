﻿using NSL.TCP.Client;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketClient.Unity
{
    public class UnityTCPClient<TClient> : TCPNetworkClient<TClient, UnityClientOptions<TClient>> 
        where TClient : BaseSocketNetworkClient, new()
    {
        public UnityTCPClient(UnityClientOptions<TClient> options)
            : base(options)
        {
        }

        protected override void OnReceive(ushort pid, int len)
        {
            ThreadHelper.InvokeOnMain(() => base.OnReceive(pid, len));
        }

        protected override void OnSend(OutputPacketBuffer rbuff, string stackTrace = "")
        {
            ThreadHelper.InvokeOnMain(() => base.OnSend(rbuff, stackTrace));
        }
    }
}
