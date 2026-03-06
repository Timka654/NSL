using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace UDPExample
{
    public class TestPacketS : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Receiver: Receive {nameof(TestPacketS)} - {data.ReadInt32()} - {data.ReadInt32()} - {data.ReadInt32()}");



            using (var packet = new NSL.UDP.DgramOutputPacketBuffer() { PacketId = 1 })
            {
                packet.WriteInt32(1);
                packet.WriteInt32(2);
                packet.WriteInt32(3);

                client.Send(packet);
            }
        }
    }
}
