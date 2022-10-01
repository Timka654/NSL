using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;

namespace Builder.TCP.BaseExample.Client
{
    [PacketTestLoad(2)]
    public class ClientTestPacket2 : IPacket<TCPNetworkClient>
    {
        public override void Receive(TCPNetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[Client]receive from {nameof(ClientTestPacket2)} - {data.ReadString16()}");
        }
    }
}
