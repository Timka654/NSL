using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.RPC
{
    public abstract class RPCHandleContainer<TClient>
        where TClient : INetworkClient
    {
        protected TClient NetworkClient { get; set; }

        public virtual void InvokeMethod(InputPacketBuffer data) 
        {

        }
    }
}