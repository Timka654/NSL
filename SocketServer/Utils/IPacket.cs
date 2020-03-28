using SocketCore.Utils.Buffer;

namespace SocketServer.Utils
{
    public interface IPacket<TClient> where TClient : IServerNetworkClient
    {
        void Receive(TClient client, InputPacketBuffer data);
    }
}
