using NSL.SocketCore.Utils;
using System.Collections.Generic;
using NSL.UDP.Enums;
using System.Net.Sockets;

namespace NSL.UDP.Channels
{
    public class ReliableChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public override UDPChannelEnum Channel => UDPChannelEnum.Reliable;

        BaseChannel<TClient, TParent> orderedChannel;
        BaseChannel<TClient, TParent> unorderedChannel;

		public ReliableChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient)
        {
			orderedChannel = new OrderedChannel<TClient, TParent>(udpClient, this);
			unorderedChannel = new UnorderedChannel<TClient, TParent>(udpClient, this);
        }

        public override void Send(UDPChannelEnum channel, byte[] data)
        {
            if (channel.HasFlag(UDPChannelEnum.Ordered))
                orderedChannel.Send(channel, data);
            else if (channel.HasFlag(UDPChannelEnum.Unordered))
                unorderedChannel.Send(channel, data);
            else
                throw new KeyNotFoundException(channel.ToString());
        }

        public override void Receive(UDPChannelEnum channel, SocketAsyncEventArgs result)
		{
			if (channel.HasFlag(UDPChannelEnum.Ordered))
				orderedChannel.Receive(channel, result);
			else if (channel.HasFlag(UDPChannelEnum.Unordered))
				unorderedChannel.Receive(channel, result);
			else
				throw new KeyNotFoundException(channel.ToString());
        }
    }
}
