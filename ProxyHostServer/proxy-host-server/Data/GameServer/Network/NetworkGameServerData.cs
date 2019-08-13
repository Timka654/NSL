using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.GameServer.Info;

namespace phs.Data.GameServer.Network
{
    /// <summary>
    /// Вся информация по текущему серверу
    /// </summary>
    public class NetworkGameServerData : INetworkClient
    {
        /// <summary>
        /// Данные о сервере
        /// </summary>
        public GameServerInfo ServerData { get;set; }
    }
}
