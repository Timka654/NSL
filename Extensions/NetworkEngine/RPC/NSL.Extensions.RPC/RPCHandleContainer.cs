using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

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