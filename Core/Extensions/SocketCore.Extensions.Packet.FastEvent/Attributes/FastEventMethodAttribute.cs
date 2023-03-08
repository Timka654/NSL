using System;

namespace NSL.SocketCore.Extensions.Packet.FastEvent.Attributes
{
    public class FastEventMethodAttribute : Attribute
    {
        public ushort PacketId { get; }

        public FastEventMethodAttribute(ushort packetId)
        {
            this.PacketId = packetId;
        }
    }
}
