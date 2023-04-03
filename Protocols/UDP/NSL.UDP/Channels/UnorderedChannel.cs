using NSL.SocketServer.Utils;
using NSL.UDP.Enums;

namespace NSL.UDP.Channels
{
    public class UnorderedChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : IServerNetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public override UDPChannelEnum Channel => UDPChannelEnum.Unordered;

        public UnorderedChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient) { }

        public UnorderedChannel(BaseUDPClient<TClient, TParent> udpClient, BaseChannel<TClient, TParent> parent) : this(udpClient)
        {
        }

        protected override void ProcessPacket(UDPChannelEnum channel, PacketReciveTemp packet)
        {
            if (!packet.Ready())
                return;

            base.ProcessPacket(channel, packet);
        }
    }
}
