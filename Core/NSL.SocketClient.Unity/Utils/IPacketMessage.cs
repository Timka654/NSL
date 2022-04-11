using SocketCore.Utils;

namespace NSL.SocketClient.Unity.Utils
{
    public class IPacketMessage<TClient, RDType> : NSL.SocketClient.Utils.IPacketMessage<TClient,RDType>
        where TClient : BaseSocketNetworkClient
    {
        public IPacketMessage(UnityClientOptions<TClient> options) : base(options)
        {
        }

        protected override void InvokeEvent(RDType data)
        {
            ThreadHelper.InvokeOnMain(()=> base.InvokeEvent(data));
        }
    }

    public class IPacketMessage<TClient> : NSL.SocketClient.Utils.IPacketMessage<TClient>
        where TClient : BaseSocketNetworkClient
    {
        public IPacketMessage(UnityClientOptions<TClient> options) : base(options)
        {
        }

        protected override void InvokeEvent()
        {
            ThreadHelper.InvokeOnMain(() => base.InvokeEvent());
        }
    }
}
