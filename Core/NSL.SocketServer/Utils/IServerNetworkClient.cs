using NSL.SocketCore;
using NSL.SocketCore.Utils;
using System;

namespace NSL.SocketServer.Utils
{
    public class BaseServerNetworkClient : IServerNetworkClient { }

    public abstract class IServerNetworkClient : INetworkClient
    {
        public override bool AliveState
        {
            get => base.LastReceiveMessage.HasValue == false || base.LastReceiveMessage.Value.AddMilliseconds(AliveCheckTimeOut) > DateTime.UtcNow;
        }

        public CoreOptions ServerOptions { get; set; }

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
