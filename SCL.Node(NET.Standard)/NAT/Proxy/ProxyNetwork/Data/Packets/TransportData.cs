using SCL.Node.Utils;
using SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets.Enums;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Unity.NAT.Proxy.ProxyNetwork.Data.Packets
{
    internal class TransportData
    {
        public static void Send(BaseSocketNetworkClient client, byte[] data)
        {
            OutputPacketBuffer packet = new OutputPacketBuffer
            {
                PacketId = (ushort)ServerPacketsEnum.Transport
            };

            packet.Write(data);

            client.Send(packet);
        }
    }
}
