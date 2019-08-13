using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using ps.Data.NodeHostClient.Info;
using ps.Data.NodeHostClient.Storages;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using ps.Data.NodeHostClient.Info.Enums.Packets;
using ps.Data.NodeServer.Info;
using ps.Data.NodeServer.Network;
using ps.Data.NodeHostClient.Info.Enums;
using Utils.Logger;

namespace ps.Data.NodeHostClient.Managers
{
    /// <summary>
    /// Контроллер для обработки комнат
    /// </summary>
    public class NodePlayerManager : NodePlayerStorage
    {
        public static NodePlayerManager Instance { get; private set; }

        public string PublicIp { get; private set; }

        public int MaxPlayerCount { get; private set; }

        public NodePlayerManager()
        {
            Instance = this;
            PublicIp = StaticData.ConfigurationManager.GetValue<string>("proxy/public.ip");
            MaxPlayerCount = StaticData.ConfigurationManager.GetValue<int>("proxy/max.client.count");

            LoggerStorage.Instance.main.AppendInfo( $"RoomManager Loaded");
        }

        public bool ConnectPlayer(NetworkClientData client)
        {
            var player = new NodePlayerInfo(client);
            AddPlayer(player);
            Packets.Player.PlayerConnected.Send(player);
            return true;
        }

        public void ConfirmPlayer(Guid guid, bool result)
        {
            var player = GetPlayer(guid);
            if (player != null)
            {
                NodeServer.Packets.Profile.LogIn.Send(player.Client, result ? LoginResultEnum.Ok : LoginResultEnum.Error);

                if (result)
                    player.Confirmed = true;
                else
                {
                    RemovePlayer(guid);
                    player.Client?.Network?.Disconnect();
                }
            }
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
        public NodePlayerInfo DisconnectPlayer(NodePlayerInfo player_info)
        {
            var player = RemovePlayer(player_info.Id);
            if (player != null)
                Packets.Player.PlayerDisconnected.Send(player);

            return player;
        }
    }
}
