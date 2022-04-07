using SCL.Node.NAT.Proxy.Data.Enums;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Node.NAT.Proxy.Data.Packets.PacketData
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
