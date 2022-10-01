using NSL.SocketClient;
using NSL.SocketServer.Utils;

namespace RPCWithBuilder.TCP.Example
{
    public class NetworkClient : BaseSocketNetworkClient
    {
        internal TestRPCClientContainerRPCRepository<NetworkClient> RPCRepository { get; } = new TestRPCClientContainerRPCRepository<NetworkClient>();
    }

    public class NetworkServerClient : IServerNetworkClient
    {
        internal TestRPCClientContainerRPCRepository<NetworkServerClient> RPCRepository { get; } = new TestRPCClientContainerRPCRepository<NetworkServerClient>();
    }
}
