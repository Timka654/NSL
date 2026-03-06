using NSL.SocketServer.Utils;
using NSL.UDP.Packet;

namespace UDPExample
{
    public class NetworkClient : IServerNetworkClient, IUDPClientWithPing<NetworkClient>
    {
        public UDPPingPacket<NetworkClient> PingPacket { get; }

        public NetworkClient()
        {
            PingPacket = new UDPPingPacket<NetworkClient>(this);
        }

        public override void Dispose()
        {
            base.Dispose();
            PingPacket.Dispose();
        }
    }
}
