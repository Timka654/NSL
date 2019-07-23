using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Node.Utils.SystemPackets
{
    public class InvalidPid
    {
        public static void Send(INodePlayer player, uint pid)
        {
            NodeOutputPacketBuffer p = new NodeOutputPacketBuffer();
            p.PacketId = byte.MaxValue;
            p.AppendHash = true;
            p.Write(pid);
            player.Send(p);
        }
    }
}
