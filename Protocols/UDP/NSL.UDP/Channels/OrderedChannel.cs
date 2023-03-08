using NSL.SocketCore.Utils;
using NSL.UDP.Enums;
using System;

namespace NSL.UDP.Channels
{
    internal class OrderedChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        private readonly BaseChannel<TClient, TParent> parent;

        public override UDPChannelEnum Channel => UDPChannelEnum.Ordered;

        public Action<PacketWaitTemp> OnSend = pid => { };

        public OrderedChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient) { }
        public OrderedChannel(BaseUDPClient<TClient, TParent> udpClient, BaseChannel<TClient, TParent> parent) : this(udpClient)
        {
            this.parent = parent;
        }

        public override void Send(UDPChannelEnum channel, byte[] data)
        {
            base.Send(channel, data);
        }

        protected override void AfterBuild(BaseChannel<TClient, TParent> fromChannel, PacketWaitTemp packet)
        {
            OnSend(packet);
            base.AfterBuild(fromChannel, packet);
        }

        internal override uint CreatePID()
            => (parent as ReliableChannel<TClient, TParent>)?.CreatePID() ?? 0;

    }
}