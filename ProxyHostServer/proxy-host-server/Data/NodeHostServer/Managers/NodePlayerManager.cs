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
    [NodeHostManagerLoad(2)]
    public class NodePlayerManager : NodePlayerStorage
    {
        public static NodePlayerManager Instance { get; private set; }

        private string accessToken { get; set; }

        public NodePlayerManager()
        {
            Instance = this;
            accessToken = StaticData.ConfigurationManager.GetValue<string>("network/node_host_server/access/token");

            LoggerStorage.Instance.main.AppendInfo($"NodePlayerManager Loaded");
        }

        internal void AppendPlayer(NetworkNodeServerData client, NodePlayerInfo player)
        {
            player.Server = client.ServerInfo;

            AddPlayer(player);

            client.ServerInfo.AddPlayer(player);
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
        public NodePlayerInfo DisconnectPlayer(Guid id)
        {
            var player = RemovePlayer(id);
            if (player != null)
                player.Server.RemovePlayer(player);

            return player;
        }

        public NodePlayerInfo DisconnectPlayer(NodePlayerInfo playerInfo)
        {
            return DisconnectPlayer(playerInfo.Id);
        }
    }
}
