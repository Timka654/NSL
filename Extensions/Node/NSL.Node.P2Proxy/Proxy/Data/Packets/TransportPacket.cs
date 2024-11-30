using NSL.Node.P2Proxy.Proxy.Data;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.P2Proxy.Client
{
    public partial class P2ProxyServerEntry
    {
        private void TransportPacketHandle(P2PNetworkClient client, InputPacketBuffer buffer)
        {
            client.Room?.Transport(client, buffer);
        }
    }
}
