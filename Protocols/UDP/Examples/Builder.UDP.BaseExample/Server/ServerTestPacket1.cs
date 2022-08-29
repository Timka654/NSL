using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSL.UDP;

namespace Builder.UDP.BaseExample.Server
{
    public class ServerTestPacket1 : IPacket<UDPServerNetworkClient>
    {
        public override void Receive(UDPServerNetworkClient client, InputPacketBuffer data)
        {
            var str = data.ReadString16();

            Console.WriteLine($"[Server]receive from {nameof(ServerTestPacket1)} - {str}");

            var pkt = new DgramPacket();

            pkt.PacketId = 1;

            pkt.WriteString16(str);

            client.Send(pkt);

            pkt = new DgramPacket();

            pkt.PacketId = 2;

            pkt.WriteString16(str);

            client.Send(pkt);

            pkt = new DgramPacket();

            pkt.PacketId = 3;

            pkt.WriteString16(str);

            client.Send(pkt);
        }
    }
}
