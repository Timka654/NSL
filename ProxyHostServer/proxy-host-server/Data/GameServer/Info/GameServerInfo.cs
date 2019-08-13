using phs.Data.GameServer.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace phs.Data.GameServer.Info
{
    public class GameServerInfo
    {
        public short Id { get; set; }

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
