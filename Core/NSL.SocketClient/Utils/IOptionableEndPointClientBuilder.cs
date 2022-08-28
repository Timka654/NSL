using NSL.EndPointBuilder;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.SocketClient.Utils
{
    public interface IOptionableEndPointClientBuilder<TClient> : IOptionableEndPointBuilder<TClient>
        where TClient : BaseSocketNetworkClient, new()
    {
        ClientOptions<TClient> GetOptions();
    }
}
