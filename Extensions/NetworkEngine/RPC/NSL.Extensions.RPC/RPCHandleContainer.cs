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
        public TClient NetworkClient { get; internal set; }

        protected virtual RPCChannelProcessor<TClient> Processor => NetworkClient.ObjectBag.Get<RPCChannelProcessor<TClient>>(RPCChannelProcessor.DefaultBagKey);

        public virtual void InvokeMethod(InputPacketBuffer data) 
        {

        }

        public virtual string GetContainerName()
            => default;
    }
}