using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;

namespace SocketServer.Utils.SystemPackets
{
    public class ServerAliveConnection<T> : IPacket<T> where T : IServerNetworkClient
    {
        public override void Receive(T client, InputPacketBuffer data)
        {
        }
    }
}
