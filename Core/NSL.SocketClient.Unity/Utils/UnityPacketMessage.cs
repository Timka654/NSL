namespace NSL.SocketClient.Unity.Utils
{
    public class UnityPacketMessage<TClient, RDType> : NSL.SocketClient.Utils.IPacketMessage<TClient,RDType>
        where TClient : BaseSocketNetworkClient
    {
        public UnityPacketMessage(UnityClientOptions<TClient> options) : base(options)
        {
        }

        protected override void InvokeEvent(RDType data)
        {
            ThreadHelper.InvokeOnMain(()=> base.InvokeEvent(data));
        }
    }

    public class UnityPacketMessage<TClient> : NSL.SocketClient.Utils.IPacketMessage<TClient>
        where TClient : BaseSocketNetworkClient
    {
        public UnityPacketMessage(UnityClientOptions<TClient> options) : base(options)
        {
        }

        protected override void InvokeEvent()
        {
            ThreadHelper.InvokeOnMain(() => base.InvokeEvent());
        }
    }
}
