using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketServer.Utils.SystemPackets
{
    public class VersionPacket<T> : IPacket<T> where T : IServerNetworkClient
    {
        public const ushort PacketId = ushort.MaxValue - 3;

        public override void Receive(T client, InputPacketBuffer data)
        {
            //client.Version = data.ReadInt64();
            // todo
        }
    }
}
