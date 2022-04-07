﻿using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketServer.Utils.SystemPackets;
using System;
using System.Threading.Tasks;

namespace SocketServer.Utils
{
    public abstract class IServerNetworkClient : INetworkClient
    {
        public override bool AliveState
        {
            get => base.LastReceiveMessage.HasValue == false || base.LastReceiveMessage.Value.AddMilliseconds(AliveCheckTimeOut) > DateTime.UtcNow;
            set => throw new InvalidOperationException();
        }

        public CoreOptions ServerOptions { get; set; }

        internal new void AddWaitPacket(byte[] packet_data, int offset, int lenght)
        {
            base.AddWaitPacket(packet_data, offset, lenght);
        }

        public void Send(OutputPacketBuffer packet)
        {
            Network.Send(packet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"> copy from</param>
        public virtual void ChangeOwner(IServerNetworkClient from)
        {
            base.ChangeOwner(from);
        }
    }
}
