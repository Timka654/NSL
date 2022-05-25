using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.SystemPackets;

namespace NSL.SocketServer.Utils.SystemPackets
{
    public class ServerAliveConnectionPacket<T> : IPacket<T> where T : IServerNetworkClient
    {
        public override void Receive(T client, InputPacketBuffer data)
        {
            client.Network.SendEmpty(AliveConnectionPacket.PacketId);
        }
    }
}
