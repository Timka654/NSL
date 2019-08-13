using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using ps.Data.NodeHostClient.Info;

namespace ps.Data.NodeHostClient.Network
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
