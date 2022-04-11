using NSL.SocketClient;
using NSL.SocketClient.Unity;
using NSL.TCP.Client;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace NSL.SocketClient.Unity
{
    public class UnityClient<TClient> : TCPNetworkClient<TClient, UnityClientOptions<TClient>> where TClient : BaseSocketNetworkClient
    {
        public UnityClient(UnityClientOptions<TClient> options)
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
