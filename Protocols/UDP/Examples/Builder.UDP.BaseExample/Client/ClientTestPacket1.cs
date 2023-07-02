using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;

namespace Builder.UDP.BaseExample.Client
{
    public class ClientTestPacket1 : IPacket<UDPTestNetworkClient>
    {
        public override void Receive(UDPTestNetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[Client]receive from {nameof(ClientTestPacket1)} - {data.ReadString()}");
        }
    }
}
