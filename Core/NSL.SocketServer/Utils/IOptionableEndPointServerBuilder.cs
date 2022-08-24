using NSL.EndPointBuilder;
using NSL.SocketCore.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.SocketServer.Utils
{
    public interface IOptionableEndPointServerBuilder<TClient> : IOptionableEndPointBuilder<TClient>
            where TClient : IServerNetworkClient, new()
    {
        ServerOptions<TClient> GetOptions();
    }
}
