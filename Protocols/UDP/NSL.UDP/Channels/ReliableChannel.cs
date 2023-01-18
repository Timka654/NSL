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

        Dictionary<UDPChannelEnum, BaseChannel<TClient, TParent>> channels = new Dictionary<UDPChannelEnum, BaseChannel<TClient, TParent>>();

        public ReliableChannel(BaseUDPClient<TClient, TParent> udpClient) : base(udpClient)
        {
            channels.Add(UDPChannelEnum.ReliableOrdered, new OrderedChannel<TClient, TParent>(udpClient, this));
            channels.Add(UDPChannelEnum.ReliableUnordered, new UnorderedChannel<TClient, TParent>(udpClient, this));
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
