using NSL.SocketServer.Utils;

namespace NSL.Node.BridgeServer.CS
{
    internal class ClientServerNetworkClient : IServerNetworkClient
    {
        public string ServerIdentity { get; set; }

        public string SessionIdentity { get; set; }
    }
}
