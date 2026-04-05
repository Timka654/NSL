using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.SocketCore.Utils.Packet.FastEvent
{
    public class EventPacket<TClient, ReceiveType> : IPacket<TClient>
        where TClient : INetworkClient
    {
        public event Action<TClient, ReceiveType> OnReceive = (_, _1) => { };

        public override void Receive(TClient client, InputPacketBuffer data) { }

        protected void InvokeEvent(TClient client, ReceiveType value)
            => OnReceive(client, value);
    }

    public class EventPacket<TClient> : IPacket<TClient>
        where TClient : INetworkClient
    {
        public event Action<TClient, InputPacketBuffer> OnReceive = (_, _1) => { };

        public override void Receive(TClient client, InputPacketBuffer data)
            => OnReceive(client, data);
    }
}
