using NSL.SocketCore.Utils;
using NSL.UDP.Enums;

namespace NSL.UDP.Channels
{
    internal class UnorderedChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        private readonly BaseChannel<TClient, TParent> parent;

        public override UDPChannelEnum Channel => UDPChannelEnum.Unordered;

        public UnorderedChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient) { }

        public UnorderedChannel(BaseUDPClient<TClient, TParent> udpClient, BaseChannel<TClient, TParent> parent) : this(udpClient)
        {
            this.parent = parent;
        }

        public override void Send(UDPChannelEnum channel, byte[] data)
        {
            base.Send(channel, data);
        }
    }
}
