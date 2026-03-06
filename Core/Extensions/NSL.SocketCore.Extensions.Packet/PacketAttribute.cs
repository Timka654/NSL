using System;

namespace NSL.SocketCore.Extensions.Packet
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class PacketAttribute : Attribute
    {
        public ushort PacketId { get; }

        public PacketAttribute(ushort packetId)
        {
            PacketId = packetId;
        }
    }
}
