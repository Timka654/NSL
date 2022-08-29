using NSL.SocketCore.Extensions.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.UDP.BaseExample.Client
{
    public class PacketTestLoadAttribute : PacketAttribute
    {
        public PacketTestLoadAttribute(ushort packetId) : base(packetId)
        {
        }
    }
}
