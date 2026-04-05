using NSL.SocketCore.Utils.Buffer;

namespace NSL.Extensions.NAT.Proxy.Data.Packets.PacketData
{
    public class ProxySignInPacketData
    {
        public string PeerId { get; set; }

        public string Token { get; set; }

        public static void WritePacketData(OutputPacketBuffer packet, ProxySignInPacketData data)
        {
            packet.WriteString(data.PeerId);
            packet.WriteString(data.Token);
        }
    }
}
