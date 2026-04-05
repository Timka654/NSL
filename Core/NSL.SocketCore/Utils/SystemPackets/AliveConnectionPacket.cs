using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Utils.SystemPackets
{
    public class AliveConnectionPacket
    {
        public const ushort PacketId = (ushort)NSLSystemPacketEnum.AliveConnection;

        public static void SendRequest(IClient client)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = PacketId
            };

            client.Send(packet);
        }
    }
}
