using NSL.SocketCore.Utils;
using NSL.UDP.Enums;

namespace NSL.UDP.Channels
{
    internal class OrderedChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        private readonly BaseChannel<TClient, TParent> parent;

        public override UDPChannelEnum Channel => UDPChannelEnum.Ordered;

        public OrderedChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient) { }
        public OrderedChannel(BaseUDPClient<TClient, TParent> udpClient, BaseChannel<TClient, TParent> parent) : this(udpClient)
        {
            this.parent = parent;
        }

        public override void Send(UDPChannelEnum channel, byte[] data)
        {
            base.Send(channel, data);
        }

        private uint CurrentPPID = 0;


    }
}