using SCL.SocketClient.Utils.Buffer;
using SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Info.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets.PacketData
{
    public class ProxySignInPacketData
    {
        public int UserId { get; set; }

        public string Session { get; set; }

        public ulong GameId { get; set; }

        internal bool Proxy { get; set; }

        public static void WritePacketData(OutputPacketBuffer packet, ProxySignInPacketData data)
        {
            packet.WriteInt32(data.UserId);
            packet.WriteUInt64(data.GameId);
            packet.WriteString16(data.Session);

            packet.WriteBool(data.Proxy);
        }
    }
}
