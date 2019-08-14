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
    [NodeHostManagerLoad(1)]
    public class ProxyServerManager : ProxyServerStorage
    {
        public static ProxyServerManager Instance { get; private set; }

        private string accessToken { get; set; }

        public ProxyServerManager()
        {
            Instance = this;

            accessToken = StaticData.ConfigurationManager.GetValue<string>("network/node_host_server/access/token");

            LoggerStorage.Instance.main.AppendInfo( $"RoomManager Loaded");
        }

        public bool ConnectServer(NetworkNodeServerData client, string connectionToken)
        {
            if (connectionToken != accessToken)
                return false;

            client.RunAliveChecker();
            AddServer(client.ServerInfo);
            //Packets.Server.ServerConnected.Send(player);
            return true;
        }

        public void ConfirmServer(short guid, bool result)
        {
            var server = GetServer(guid);
            if (server != null)
            {
                //NodeHostServer.Packets.Profile.LogIn.Send(player.Client, result ? LoginResultEnum.Ok : LoginResultEnum.Error);

                //if (result)
                //    server.Confirmed = true;
                //else
                //{
                    RemoveServer(guid);
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
        public ProxyServerInfo DisconnectServer(ProxyServerInfo serverInfo)
        {
            var server = RemoveServer(serverInfo.Id);
            //if (server != null)
            //    Packets.Server.ServerDisconnected.Send(server);

            return server;
        }
    }
}
