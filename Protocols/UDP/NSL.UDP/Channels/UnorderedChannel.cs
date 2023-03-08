using NSL.SocketCore.Utils;
using NSL.UDP.Enums;
using System;

namespace NSL.UDP.Channels
{
    internal class UnorderedChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        private readonly BaseChannel<TClient, TParent> parent;

        public override UDPChannelEnum Channel => UDPChannelEnum.Unordered;

        public Action<PacketWaitTemp> OnSend = pid => { };

        public UnorderedChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient) { }

        public UnorderedChannel(BaseUDPClient<TClient, TParent> udpClient, BaseChannel<TClient, TParent> parent) : this(udpClient)
        {
            this.parent = parent;
        }

        protected override void AfterBuild(BaseChannel<TClient, TParent> fromChannel, PacketWaitTemp packet)
        {
            OnSend(packet);
            base.AfterBuild(fromChannel, packet);
        }

        public override void Send(UDPChannelEnum channel, byte[] data)
        {
            base.Send(channel, data);
        }

        internal override uint CreatePID()
            => (parent as ReliableChannel<TClient, TParent>)?.CreatePID() ?? 0;
    }
}
