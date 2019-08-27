using phs.Data.NodeHostServer.Network;
using phs.Data.GameServer.Info;
using Utils;
using System;
using System.Collections.Generic;
using System.Text;
using SocketServer.Utils.Buffer;
using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System.Collections.Concurrent;

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

        private ConcurrentDictionary<Guid, NodePlayerInfo> playerMap = new ConcurrentDictionary<Guid, NodePlayerInfo>();

        /// <summary>
        /// Текущий клиент для передачи данных
        /// </summary>
        public NetworkNodeServerData Client { get; private set; }

        public ProxyServerInfo(NetworkNodeServerData client)
        {
            Client = client;
        }

        internal void AddPlayer(NodePlayerInfo player)
        {
            playerMap.TryAdd(player.Id, player);
        }

        internal void RemovePlayer(NodePlayerInfo player)
        {
            playerMap.Remove(player.Id, out player);
            Packets.Player.PlayerConnected.Send(player, false);
        }

        internal void DisconnectAll()
        {
            foreach (var item in playerMap)
            {
                StaticData.NodePlayerManager.DisconnectPlayer(item.Value);
            }
            playerMap.Clear();
        }

        internal void ConfirmPlayer(NodePlayerInfo player)
        {
            Packets.Player.PlayerConnected.Send(player, true);
        }

        public void ChangeClient(NetworkNodeServerData client)
        {
            Client = client;
        }
    }
}
