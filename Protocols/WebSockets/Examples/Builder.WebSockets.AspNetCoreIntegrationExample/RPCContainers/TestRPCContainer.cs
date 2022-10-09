using NSL.Extensions.RPC;
using NSL.Extensions.RPC.Generator.Attributes;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Server.AspNetPoint;

namespace Builder.WebSockets.AspNetCoreIntegrationExample.RPCContainers
{
    public class TestRPCContainer<TClient> : RPCHandleContainer<TClient>
        where TClient : INetworkClient
    {
        [RPCMethod]
        public virtual int test(int value)
        {
            return (value * 2);
        }
    }
}
