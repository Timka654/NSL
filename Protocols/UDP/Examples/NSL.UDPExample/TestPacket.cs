using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace UDPExample
{
    public class TestPacket : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Client: Receive {nameof(TestPacket)} - {data.ReadInt32()} - {data.ReadInt32()} - {data.ReadInt32()}");
        }
    }
}
