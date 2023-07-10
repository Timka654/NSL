using NSL.SocketCore.Utils.Buffer;

namespace NSL.Extensions.Version.Client.Packets
{
    public class VersionPacket
    {
        public const ushort PacketId = ushort.MaxValue - 3;

        public static void Send(BaseSocketNetworkClient client, long version)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = PacketId
            };

            packet.WriteInt64(version);

            client.Network.Send(packet);
        }
    }
}
