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
    public class NetworkNodeHostClientData : INetworkClient
    {
        /// <summary>
        /// Данные о сервере
        /// </summary>
        public NodeHostClientInfo ServerData { get;set; }
    }
}
