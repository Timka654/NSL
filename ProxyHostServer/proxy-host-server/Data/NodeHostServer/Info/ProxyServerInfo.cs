using phs.Data.NodeHostServer.Network;
using phs.Data.GameServer.Info;
using Utils;
using System;
using System.Collections.Generic;
using System.Text;
using SocketServer.Utils.Buffer;
using BinarySerializer;
using BinarySerializer.DefaultTypes;

namespace phs.Data.NodeHostServer.Info
{
    /// <summary>
    /// Общие данные для игроков в лобби разных типов боя
    /// </summary>
    public class ProxyServerInfo
    {
        public short Id { get; set; }

        public string Ip { get; set; }

        public int Port { get; set; }

        public int MaxPlayerCount { get; set; }

        /// <summary>
        /// Текущий клиент для передачи данных
        /// </summary>
        public NetworkNodeServerData Client { get; private set; }

        public ProxyServerInfo(NetworkNodeServerData client)
        {
            Client = client;
        }
    }
}
