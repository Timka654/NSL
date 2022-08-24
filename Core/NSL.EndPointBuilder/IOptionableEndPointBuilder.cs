using NSL.SocketCore.Utils;
using NSL.SocketCore;
using NSL.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.EndPointBuilder
{
    public interface IOptionableEndPointBuilder<TClient> : IEndPointBuilder
        where TClient : INetworkClient, new()
    {
        CoreOptions<TClient> GetCoreOptions();
    }
}
