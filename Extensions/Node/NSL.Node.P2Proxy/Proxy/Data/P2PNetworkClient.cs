using NSL.SocketServer.Utils;
using System;

namespace NSL.Node.P2Proxy.Proxy.Data
{
    public class P2PNetworkClient : IServerNetworkClient
    {
        public Guid Id { get; set; }

        public ProxyRoomInfo Room { get; set; }
    }
}
