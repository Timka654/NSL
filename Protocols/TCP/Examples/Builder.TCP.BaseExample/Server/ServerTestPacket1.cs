using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;

namespace Builder.TCP.BaseExample.Server
{
    public class ServerTestPacket1 : IPacket<TCPServerNetworkClient>
    {
        public override void Receive(TCPServerNetworkClient client, InputPacketBuffer data)
        {
            var str = data.ReadString16();

            Console.WriteLine($"[Server]receive from {nameof(ServerTestPacket1)} - {str}");

            var pkt = new OutputPacketBuffer();

            pkt.PacketId = 1;

            pkt.WriteString16(str);

            client.Send(pkt);

            pkt = new OutputPacketBuffer();

            pkt.PacketId = 2;

            pkt.WriteString16(str);

            client.Send(pkt);

            pkt = new OutputPacketBuffer();

            pkt.PacketId = 3;

            pkt.WriteString16(str);

            client.Send(pkt);
        }
    }
}
