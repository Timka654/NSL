﻿using SocketCore.Extensions.Buffer;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;

namespace SocketCore.Extensions.Packet.FastEvent
{
    public class EventPacket<TClient, ReceiveType> : IPacket<TClient>
        where TClient : INetworkClient
    {
        public event Action<TClient, ReceiveType> OnReceive = (_, _1) => { };

        public override void Receive(TClient client, InputPacketBuffer data)
        {
        }

        protected void InvokeEvent(TClient client, ReceiveType value)
        {
            OnReceive(client, value);
        }
    }

    public class EventPacket<TClient> : IPacket<TClient>
        where TClient : INetworkClient
    {
        public event Action<TClient, InputPacketBuffer> OnReceive = (_, _1) => { };

        public override void Receive(TClient client, InputPacketBuffer data)
        {
            OnReceive(client, data);
        }
    }

    public class EventJson16Packet<TClient, ReceiveType> : EventPacket<TClient, ReceiveType>
        where TClient : INetworkClient
    {
        public override void Receive(TClient client, InputPacketBuffer data)
        {
            InvokeEvent(client, data.ReadJson16<ReceiveType>());
        }
    }

    public class EventJson32Packet<TClient, ReceiveType> : EventPacket<TClient, ReceiveType>
        where TClient : INetworkClient
    {
        public override void Receive(TClient client, InputPacketBuffer data)
        {
            InvokeEvent(client, data.ReadJson32<ReceiveType>());
        }
    }
}
