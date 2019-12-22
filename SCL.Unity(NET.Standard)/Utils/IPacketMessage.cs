using System;
using System.Collections.Generic;
using System.Text;

namespace SCL.Unity.Utils
{
    public class IPacketMessage<TClient, RDType> : SCL.Utils.IPacketMessage<TClient,RDType>
        where TClient : BaseSocketNetworkClient
    {
        public IPacketMessage(ClientOptions<TClient> options) : base(options)
        {
        }

        protected override void InvokeEvent(RDType data)
        {
            ThreadHelper.InvokeOnMain(()=> base.InvokeEvent(data));
        }
    }

    public class IPacketMessage<TClient> : SCL.Utils.IPacketMessage<TClient>
        where TClient : BaseSocketNetworkClient
    {
        public IPacketMessage(ClientOptions<TClient> options) : base(options)
        {
        }

        protected override void InvokeEvent()
        {
            ThreadHelper.InvokeOnMain(() => base.InvokeEvent());
        }
    }
}
