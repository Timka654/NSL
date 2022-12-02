using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.SocketServer.Utils
{
    public abstract class IServerNetworkClient : INetworkClient
    {
        public override bool AliveState
        {
            get => base.LastReceiveMessage.HasValue == false || base.LastReceiveMessage.Value.AddMilliseconds(AliveCheckTimeOut) > DateTime.UtcNow;
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
