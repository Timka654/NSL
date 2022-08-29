using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.UDP.BaseExample.Client
{
    public class ClientTestPacket1 : IPacket<UDPTestNetworkClient>
    {
        public override void Receive(UDPTestNetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[Client]receive from {nameof(ClientTestPacket1)} - {data.ReadString16()}");
        }
    }
}
