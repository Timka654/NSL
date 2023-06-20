using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;

namespace Builder.WebSockets.BaseExample.Server
{
    public class ServerTestPacket1 : IPacket<WebSocketsServerNetworkClient>
    {
        public override void Receive(WebSocketsServerNetworkClient client, InputPacketBuffer data)
        {
            var str = data.ReadString();

            Console.WriteLine($"[Server]receive from {nameof(ServerTestPacket1)} - {str}");

            var pkt = new OutputPacketBuffer();

            pkt.PacketId = 1;

            pkt.WriteString(str);

            client.Send(pkt);

            pkt = new OutputPacketBuffer();

            pkt.PacketId = 2;

            pkt.WriteString(str);

            client.Send(pkt);

            pkt = new OutputPacketBuffer();

            pkt.PacketId = 3;

            pkt.WriteString(str);

            client.Send(pkt);
        }
    }
}
