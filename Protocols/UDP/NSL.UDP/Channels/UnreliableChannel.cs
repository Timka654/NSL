﻿using NSL.SocketCore.Utils;
using NSL.UDP.Enums;
using System;
using System.Collections.Generic;

namespace NSL.UDP.Channels
{
    internal class UnreliableChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public override UDPChannelEnum Channel => UDPChannelEnum.Unreliable;

		BaseChannel<TClient, TParent> orderedChannel;
		BaseChannel<TClient, TParent> unorderedChannel;

		public UnreliableChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient)
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

		public override void Receive(UDPChannelEnum channel, Span<byte> data)
		{
			if (channel.HasFlag(UDPChannelEnum.Ordered))
				orderedChannel.Receive(channel, data);
			else if (channel.HasFlag(UDPChannelEnum.Unordered))
				unorderedChannel.Receive(channel, data);
			else
				throw new KeyNotFoundException(channel.ToString());
		}
	}
}