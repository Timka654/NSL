using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using phs.Data.NodeHostServer.Info;
using phs.Data.NodeHostServer.Network;
using Utils.Logger;
using phs.Data.NodeHostServer.Storages;

namespace phs.Data.NodeHostServer.Managers
{
    /// <summary>
    /// Контроллер для обработки комнат
    /// </summary>
    [NodeHostManagerLoad(1)]
    public class ProxyServerManager : ProxyServerStorage
    {
        public static ProxyServerManager Instance { get; private set; }

        private string accessToken { get; set; }

        public ProxyServerManager()
        {
            Instance = this;

            accessToken = StaticData.ConfigurationManager.GetValue<string>("network/node_host_server/access/token");

            LoggerStorage.Instance.main.AppendInfo( $"RoomManager Loaded");
        }

        public bool ConnectServer(NetworkNodeServerData client, bool firstLoading, string connectionToken)
        {
            if (connectionToken != accessToken)
                return false;

            client.RunAliveChecker();

            if (firstLoading)
                DisconnectServer(client.ServerInfo.Id);
            else
            {
                var server = GetServer(client.ServerInfo.Id);

                if(server != null)
                {
                    client.ServerInfo = server;
                    server.ChangeClient(client);
                    return true;
                }
            }

            AddServer(client.ServerInfo);
            return true;
        }

        public ProxyServerInfo DisconnectServer(ProxyServerInfo serverInfo)
        {
            return DisconnectServer(serverInfo.Id);
        }

        public ProxyServerInfo DisconnectServer(short id)
        {
            var server = RemoveServer(id);
            if (server != null)
                server.DisconnectAll();

            return server;
        }
    }
}
