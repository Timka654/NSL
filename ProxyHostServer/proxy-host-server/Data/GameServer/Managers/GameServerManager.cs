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
using phs.Data.GameServer.Info;
using phs.Data.GameServer.Network;
using phs.Data.GameServer.Info.Enums;
using Utils.Logger;

namespace phs.Data.GameServer.Managers
{
    /// <summary>
    /// Контроллер для обработки комнат
    /// </summary>
    public class GameServerManager : GameServerStorage
    {
        public static GameServerManager Instance { get; private set; }

        public GameServerManager()
        {
            Instance = this;

            LoggerStorage.Instance.main.AppendInfo( $"RoomManager Loaded");
        }

        public bool ConnectServer(NetworkGameServerData client)
        {
            var player = new GameServerInfo(client);
            AddServer(player);
            //Packets.Player.PlayerConnected.Send(player);
            return true;
        }

        public void ConfirmPlayer(short id, bool result)
        {
            var server = GetServer(id);
            if (server != null)
            {
                //NodeHostServer.Packets.Profile.LogIn.Send(player.Client, result ? LoginResultEnum.Ok : LoginResultEnum.Error);

                //if (result)
                //    //server.Confirmed = true;
                //else
                //{
                    RemoveServer(id);
                    server.Client?.Network?.Disconnect();
                //}
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
        public GameServerInfo DisconnectServer(GameServerInfo player_info)
        {
            var player = RemoveServer(player_info.Id);
            //if (player != null)
            //    Packets.Player.PlayerDisconnected.Send(player);

            return player;
        }
    }
}
