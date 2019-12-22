//using ReliableNetcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Node.Utils
{
    public abstract class INodePlayer
    {
        //public virtual void Send(NodeOutputPacketBuffer p, QosType qos) { throw new NotImplementedException(); }
        
        public virtual int PlayerId { get; internal set; }

        public virtual INetworkNode NetworkNode { get; set; }
    }
}
