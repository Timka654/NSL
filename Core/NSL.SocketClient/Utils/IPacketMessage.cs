namespace NSL.SocketClient.Utils
{
    public class IPacketMessage<TClient, RDType> : IClientPacket<TClient>
        where TClient : BaseSocketNetworkClient
    {
        public IPacketMessage(ClientOptions<TClient> options) : base(options)
        {
        }

        public delegate void OnReceiveEventHandle(RDType value);
        public virtual event OnReceiveEventHandle OnReceiveEvent;

        protected virtual void InvokeEvent(RDType data)
        {
            OnReceiveEvent?.Invoke(data);
        }
    }

    public class IPacketMessage<TClient> : IClientPacket<TClient>
        where TClient : BaseSocketNetworkClient
    {
        public IPacketMessage(ClientOptions<TClient> options) : base(options)
        {
        }

        public delegate void OnReceiveEventHandle();
        public virtual event OnReceiveEventHandle OnReceiveEvent;

        protected virtual void InvokeEvent()
        {
            OnReceiveEvent?.Invoke();
        }
    }
}
