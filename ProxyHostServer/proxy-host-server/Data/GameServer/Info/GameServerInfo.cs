using phs.Data.GameServer.Network;
using phs.Data.NodeHostServer.Info;
using System;
using System.Collections.Generic;
using System.Text;

namespace phs.Data.GameServer.Info
{
    public class GameServerInfo
    {
        public int Id { get; set; }

        /// <summary>
        /// Текущий клиент для передачи данных
        /// </summary>
        public NetworkGameServerData Client { get; private set; }

        public GameServerInfo(NetworkGameServerData client)
        {
            Client = client;
        }
    }
}
