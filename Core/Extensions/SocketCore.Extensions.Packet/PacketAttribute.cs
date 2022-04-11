using System;

namespace ServerOptions.Extensions.Packet
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class PacketAttribute : Attribute
    {
        public ushort PacketId { get; set; }

        public PacketAttribute(ushort packetId)
        {
            PacketId = packetId;
        }
    }
}
