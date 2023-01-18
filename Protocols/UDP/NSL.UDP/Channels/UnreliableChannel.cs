using NSL.SocketCore.Utils;
using NSL.UDP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NSL.UDP.Channels
{
    internal class UnreliableChannel<TClient, TParent> : BaseChannel<TClient, TParent>
        where TClient : INetworkClient
        where TParent : BaseUDPClient<TClient, TParent>
    {
        public override UDPChannelEnum Channel => UDPChannelEnum.Unreliable;

        Dictionary<UDPChannelEnum, BaseChannel<TClient, TParent>> channels = new Dictionary<UDPChannelEnum, BaseChannel<TClient, TParent>>();

        public UnreliableChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient)
        {
            channels.Add(UDPChannelEnum.UnreliableOrdered, new OrderedChannel<TClient, TParent>(udpClient, this));
            channels.Add(UDPChannelEnum.UnreliableUnordered, new UnorderedChannel<TClient, TParent>(udpClient, this));
        }

        public override void Send(UDPChannelEnum channel, byte[] data)
        {
            if (!channels.TryGetValue(channel, out var c_channel))
                throw new KeyNotFoundException();

            c_channel.Send(channel, data);
        }

        public override void Receive(UDPChannelEnum channel, SocketAsyncEventArgs result)
        {
            if (channels.TryGetValue(channel, out var ch))
                ch.Receive(channel, result);
        }
    }
}
