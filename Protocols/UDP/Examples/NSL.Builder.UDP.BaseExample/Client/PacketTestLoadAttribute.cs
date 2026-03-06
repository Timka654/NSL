using NSL.SocketCore.Extensions.Packet;

namespace Builder.UDP.BaseExample.Client
{
    public class PacketTestLoadAttribute : PacketAttribute
    {
        public PacketTestLoadAttribute(ushort packetId) : base(packetId)
        {
        }
    }
}
