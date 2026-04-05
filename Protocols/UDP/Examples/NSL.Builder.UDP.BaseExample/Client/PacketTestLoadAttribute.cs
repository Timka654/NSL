using NSL.SocketCore.Utils.Packet;

namespace NSL.Builder.UDP.BaseExample.Client
{
    public class PacketTestLoadAttribute : PacketAttribute
    {
        public PacketTestLoadAttribute(ushort packetId) : base(packetId)
        {
        }
    }
}
