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
using System.Net;
using System.Collections.Concurrent;

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

            LoggerStorage.Instance.main.AppendInfo($"RoomManager Loaded");
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
                if (result)
                {
                    player.Confirmed = true;
                    player.ProxyIp = GenerateIPv6();
                }

                NodeServer.Packets.Profile.LogIn.Send(player, result ? LoginResultEnum.Ok : LoginResultEnum.Error);

                if (!result)
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
        public NodePlayerInfo DisconnectPlayer(NodePlayerInfo playerInfo)
        {
            var player = RemovePlayer(playerInfo.Id);
            if (player != null)
                Packets.Player.PlayerDisconnected.Send(player);

            return player;
        }

        private string GenerateIPv6()
        {
            byte[] bytes = new byte[16];
            new Random().NextBytes(bytes);
            IPAddress ipv6Address = new IPAddress(bytes);
            return ipv6Address.ToString();
        }

        internal void Transport(NetworkClientData client, InputPacketBuffer data)
        {
            if (client.FavorList == null)
                client.FavorList = new ConcurrentBag<NetworkClientData>(PlayersWhere(x => x.Client.RoomId == client.RoomId && x.Client.ServerId == client.ServerId && ).Select(x => x.Client));
            byte[] body = data.GetBody();
            foreach (var item in client.FavorList)
            {
                item.Network.Send(body, 0, body.Length);
            }
        }
    }
}
