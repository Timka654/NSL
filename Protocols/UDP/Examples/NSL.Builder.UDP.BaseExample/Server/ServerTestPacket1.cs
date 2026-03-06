using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;

namespace Builder.UDP.BaseExample.Server
{
    public class ServerTestPacket1 : IPacket<UDPServerNetworkClient>
    {
        public override void Receive(UDPServerNetworkClient client, InputPacketBuffer data)
        {
            var str = data.ReadString();

            Console.WriteLine($"[Server]receive from {nameof(ServerTestPacket1)} - {str}");

            var pkt = new NSL.UDP.DgramOutputPacketBuffer();

            pkt.PacketId = 1;

            pkt.WriteString(str);

            client.Send(pkt);

            pkt = new NSL.UDP.DgramOutputPacketBuffer();

            pkt.PacketId = 2;

            pkt.WriteString(str);

            client.Send(pkt);

            pkt = new NSL.UDP.DgramOutputPacketBuffer();

            pkt.PacketId = 3;

            pkt.WriteString(str);

            client.Send(pkt);
        }
    }
}
