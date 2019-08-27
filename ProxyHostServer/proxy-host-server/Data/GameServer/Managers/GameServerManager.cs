using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.GameServer.Info;
using phs.Data.GameServer.Storages;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using phs.Data.GameServer.Info.Enums.Packets;
using phs.Data.GameServer.Network;
using phs.Data.GameServer.Info.Enums;
using Utils.Logger;
using phs.Data.NodeHostServer.Info;

namespace phs.Data.GameServer.Managers
{
    /// <summary>
    /// Контроллер для обработки комнат
    /// </summary>
    public class GameServerManager : GameServerStorage
    {
        public static GameServerManager Instance { get; private set; }

        private string accessToken { get; set; }

        public GameServerManager()
        {
            Instance = this;

            accessToken = StaticData.ConfigurationManager.GetValue<string>("network/node_host_server/access/token");

            LoggerStorage.Instance.main.AppendInfo($"RoomManager Loaded");
        }

        public bool ConnectServer(NetworkGameServerData client, string connectionToken)
        {
            if (accessToken != connectionToken)
                return false;

            client.RunAliveChecker();
            AddServer(client.ServerData);

            return true;
        }

        internal void PlayerDisconnect(NetworkGameServerData server, Guid guid)
        {
            Packets.Player.PlayerDisconnected.Send(server, guid);
        }

        /// <summary>
        /// Отключение пользователя (выход из боя)
        /// </summary>
        /// <param name="user_id">Идентификатор пользователя</param>
        /// <param name="character_id">Идентификатор персонажа</param>
        /// <param name="room_id">Идентификатор комнаты</param>
        /// <param name="kill_count">Кол-во убийств</param>
        /// <param name="death_count">Кол-во смертей</param>
        /// <param name="score">Очки</param>
        /// <param name="item_list">Список с данными об износе вещей</param>
        public GameServerInfo DisconnectServer(GameServerInfo player_info)
        {
            var player = RemoveServer(player_info.Id);

            return player;
        }

        public void PlayerConnect(NetworkGameServerData server, NodePlayerInfo player)
        {
            Packets.Player.PlayerConnected.Send(server, player);
        }
    }
}
