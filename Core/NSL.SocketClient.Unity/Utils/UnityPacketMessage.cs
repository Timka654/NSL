using NSL.SocketClient.Utils;
using NSL.SocketCore.Utils;
using NSL.Utils.Unity;

namespace NSL.SocketClient.Unity.Utils
{
    public class IUnityPacketMessage<TClient, RDType> : IPacketMessage<TClient,RDType>
        where TClient : BaseNetworkConnection, new()
    {
        public IUnityPacketMessage(ClientOptions<TClient> options) : base(options)
        {
        }

        protected override void InvokeEvent(RDType data)
        {
            ThreadHelper.InvokeOnMain(()=> base.InvokeEvent(data));
        }
    }

    public class IUnityPacketMessage<TClient> : IPacketMessage<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        public IUnityPacketMessage(ClientOptions<TClient> options) : base(options)
        {
        }

        protected override void InvokeEvent()
        {
            ThreadHelper.InvokeOnMain(() => base.InvokeEvent());
        }
    }
}
