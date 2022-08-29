using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.WebSockets.BaseExample.Client
{
    [PacketTestLoad(2)]
    public class ClientTestPacket2 : IPacket<WebSocketsNetworkClient>
    {
        public override void Receive(WebSocketsNetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[Client]receive from {nameof(ClientTestPacket2)} - {data.ReadString16()}");
        }
    }
}
