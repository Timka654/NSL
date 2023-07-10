using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;

namespace NSL.Extensions.Version.Server.Packets
{
    public class VersionPacket<T> : IPacket<T> where T : IServerNetworkClient
    {
        public const ushort PacketId = ushort.MaxValue - 3;

        public override void Receive(T client, InputPacketBuffer data)
        {
            var response = data.CreateResponse();

            //client.Version = data.ReadInt64();
            // todo
        }
    }
}
