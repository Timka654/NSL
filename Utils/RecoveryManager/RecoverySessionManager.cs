using SocketServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    public class RecoverySessionManager
    {

        public void RegisterServer(ServerOptions<SocketServer.Utils.INetworkClient> server)
        {
            server.OnClientDisconnectEvent += Server_OnClientDisconnectEvent;
        }

        private void Server_OnClientDisconnectEvent(SocketServer.Utils.INetworkClient client)
        {

        }
    }
}
