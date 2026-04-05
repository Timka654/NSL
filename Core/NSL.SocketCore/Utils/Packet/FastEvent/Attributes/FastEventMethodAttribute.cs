using System;

namespace NSL.SocketCore.Utils.Packet.FastEvent
{
    public class FastEventMethodAttribute : Attribute
    {
        public ushort PacketId { get; }

        public FastEventMethodAttribute(ushort packetId)
        {
            PacketId = packetId;
        }
    }
}
