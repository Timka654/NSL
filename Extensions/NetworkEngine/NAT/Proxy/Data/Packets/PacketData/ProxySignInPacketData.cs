using NSL.SocketCore.Utils.Buffer;

namespace NSL.Extensions.NAT.Proxy.Data.Packets.PacketData
{
    public class ProxySignInPacketData
    {
        public string UserId { get; set; }

        public string GameId { get; set; }

        public string Session { get; set; }

        public static void WritePacketData(OutputPacketBuffer packet, ProxySignInPacketData data)
        {
            packet.WriteString16(data.UserId);
            packet.WriteString16(data.GameId);
            packet.WriteString16(data.Session);
        }
    }
}
