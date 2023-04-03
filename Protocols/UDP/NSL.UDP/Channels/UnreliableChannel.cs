using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using NSL.UDP.Enums;
using System;
using System.Collections.Generic;

namespace NSL.UDP.Channels
{
    public class UnreliableChannel<TClient, TParent> : UnorderedChannel<TClient, TParent>
        where TClient : IServerNetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public override UDPChannelEnum Channel => UDPChannelEnum.Unreliable | UDPChannelEnum.Unordered;
		public UnreliableChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient)
		{
		}

		public override void Send(UDPChannelEnum channel, byte[] data)
		{
			if (channel.HasFlag(UDPChannelEnum.Unordered))
                base.Send(channel, data);
			else
				throw new KeyNotFoundException(channel.ToString());
		}
	}
}
