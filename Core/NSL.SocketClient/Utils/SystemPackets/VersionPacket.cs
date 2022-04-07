using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;

namespace SocketClient.Utils.SystemPackets
{
    public class VersionPacket
    {
        public static void Send(BaseSocketNetworkClient client, long version)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)ServerPacketEnum.Version
            };

            packet.WriteInt64(version);

            client.Network.Send(packet);
        }
    }
}
