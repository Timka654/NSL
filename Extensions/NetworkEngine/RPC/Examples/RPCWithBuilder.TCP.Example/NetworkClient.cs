using NSL.SocketClient;
using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
