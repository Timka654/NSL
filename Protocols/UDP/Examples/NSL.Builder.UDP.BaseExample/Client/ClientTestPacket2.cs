using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;

namespace Builder.UDP.BaseExample.Client
{
    [PacketTestLoad(2)]
    public class ClientTestPacket2 : IPacket<UDPTestNetworkClient>
    {
        public override void Receive(UDPTestNetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[Client]receive from {nameof(ClientTestPacket2)} - {data.ReadString()}");
        }
    }
}
