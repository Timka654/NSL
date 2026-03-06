using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;

namespace Builder.WebSockets.BaseExample.Client
{
    public class ClientTestPacket1 : IPacket<WebSocketsNetworkClient>
    {
        public override void Receive(WebSocketsNetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[Client]receive from {nameof(ClientTestPacket1)} - {data.ReadString()}");
        }
    }
}
