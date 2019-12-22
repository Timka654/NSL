using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Utils
{
    public class IPacketMessage<TClient, RDType> : IPacket<TClient>
        where TClient : BaseSocketNetworkClient
    {
        public IPacketMessage(ClientOptions<TClient> options) : base(options)
        {
        }

        public delegate void OnReceiveEventHandle(RDType value);
        public virtual event OnReceiveEventHandle OnReceiveEvent;

        protected void InvokeEvent(RDType data)
        {
            ThreadHelper.InvokeOnMain(() => { OnReceiveEvent?.Invoke(data); });
        }
    }

    public class IPacketMessage<TClient> : IPacket<TClient>
        where TClient : BaseSocketNetworkClient
    {
        public IPacketMessage(ClientOptions<TClient> options) : base(options)
        {
        }

        public delegate void OnReceiveEventHandle();
        public virtual event OnReceiveEventHandle OnReceiveEvent;

        protected void InvokeEvent()
        {
            ThreadHelper.InvokeOnMain(() => { OnReceiveEvent?.Invoke(); });
        }
    }
}
