using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Node
{
    public enum NodeSystemPacketEnum : ushort
    {

        RPCCall = ushort.MaxValue - 1,
        Authorize = ushort.MaxValue
    }
}
