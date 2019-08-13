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
    public class NodePlayerInfo
    {
        public Guid Id { get; private set; }

        protected NetworkClientData _client;

        public bool Confirmed { get; set; }

        /// <summary>
        /// Текущий клиент для передачи данных
        /// </summary>
        public NetworkClientData Client => _client;

        public NodePlayerInfo(NetworkClientData client)
        {
            _client = client;
            Id = Guid.NewGuid();
        }
    }
}
