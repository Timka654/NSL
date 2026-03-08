using NSL.SocketCore.Extensions.Packet;

namespace NSL.Builder.WebSockets.BaseExample.Client
{
    public class PacketTestLoadAttribute : PacketAttribute
    {
        public PacketTestLoadAttribute(ushort packetId) : base(packetId)
        {
        }
    }
}
