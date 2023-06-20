using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;

namespace Builder.TCP.BaseExample.Client
{
    public class ClientTestPacket1 : IPacket<TCPNetworkClient>
    {
        public override void Receive(TCPNetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[Client]receive from {nameof(ClientTestPacket1)} - {data.ReadString()}");
        }
    }
}
