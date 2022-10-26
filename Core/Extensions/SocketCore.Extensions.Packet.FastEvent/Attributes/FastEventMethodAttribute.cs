using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
