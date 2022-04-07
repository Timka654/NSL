//using ReliableNetcode;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Node.Utils
{
    public abstract class INodePlayer
    {
        public virtual string PlayerId { get; internal set; }

        public virtual IServerNetworkClient Network { get; set; }
    }
}
