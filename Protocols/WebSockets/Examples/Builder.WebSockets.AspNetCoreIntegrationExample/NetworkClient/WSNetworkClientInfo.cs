using NSL.SocketClient;

namespace Builder.WebSockets.AspNetCoreIntegrationExample.NetworkClient
{
    public class WSNetworkClientInfo : BaseSocketNetworkClient
    {
        public RPCContainers.TestRPCContainerRPCRepository<WSNetworkClientInfo> TestRepo = new RPCContainers.TestRPCContainerRPCRepository<WSNetworkClientInfo>();
    }
}
