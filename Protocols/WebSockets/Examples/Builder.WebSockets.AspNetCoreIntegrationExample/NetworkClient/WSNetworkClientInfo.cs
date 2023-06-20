using NSL.SocketClient;
using Builder.WebSockets.AspNetCoreIntegrationExample.RPCContainers;

namespace Builder.WebSockets.AspNetCoreIntegrationExample.NetworkClient
{
    public class WSNetworkClientInfo : BaseSocketNetworkClient
    {
        public TestRPCContainerRPCRepository<WSNetworkClientInfo> TestRepo = new TestRPCContainerRPCRepository<WSNetworkClientInfo>();
    }
}
