using Cipher;
using SocketServer;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.NodeHostServer.Network;
using phs.Data.GameServer.Packets;
using System.Net;
using System.Net.Sockets;
using phs.Data.GameServer.Info;
using phs.Data.GameServer.Info.Enums;
using phs.Data.GameServer.Managers;
using Utils.Logger;
using Utils.Helper.Network;
using SocketServer.Utils;
using Utils.Helper;

namespace phs.Data.GameServer.Network
{
    /// <summary>
    /// Клиент приема подключений Game Host серверов
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Глобальные настройки сервера
        /// </summary>
        public static ServerOptions<NetworkGameServerData> options;

        /// <summary>
        /// Слушатель подключений
        /// </summary>
        public static ServerListener<NetworkGameServerData> listener;

        #region Load

        /// <summary>
        /// Полная инициализация
        /// </summary>
        public static void Load()
        {
            LoggerStorage.Instance.main.AppendInfo( $"---> Game Server Network Loading");

            LoadConfiguration();

            LoadManagers();

            Structorian.BuildStructures(BinarySerializer.TypeStorage.Instance);

            LoadReceivePackets();

            LoadListener();

            LoggerStorage.Instance.main.AppendInfo( $"---> Game Server Network Loaded");
        }

        /// <summary>
        /// Загрузка конфигурации, установка handle
        /// </summary>
        private static void LoadConfiguration()
        {
            LoggerStorage.Instance.main.AppendInfo( $"-> Configuration Loading");

            options = StaticData.ConfigurationManager.LoadConfigurationServerOptions<NetworkGameServerData>("network/game_server");

            options.inputCipher = new PacketNoneCipher();
            options.outputCipher = new PacketNoneCipher();

            options.OnClientConnectEvent += SocketOptions_OnClientConnectEvent;
            options.OnClientDisconnectEvent += SocketOptions_OnClientDisconnectEvent;
            options.OnExtensionEvent += SocketOptions_OnExtensionEvent;

            LoggerStorage.Instance.main.AppendInfo( $"-> Configuration Loaded");
        }

        /// <summary>
        /// Загрузка менеджеров(Обработчиков)
        /// </summary>
        private static void LoadManagers()
        {
            LoggerStorage.Instance.main.AppendInfo( $"-> Managers Loading");

            new GameServerManager();

            LoggerStorage.Instance.main.AppendInfo( $"-> Managers Loaded");
        }

        /// <summary>
        /// Загрузка пакетов для приема с клиента
        /// </summary>
        private static void LoadReceivePackets()
        {
            LoggerStorage.Instance.main.AppendInfo( $"-> Game Server Network Packets Loading");

            options.LoadPackets(typeof(GamePacketAttribute));

            LoggerStorage.Instance.main.AppendInfo( $"-> Game Server Network Packets Loaded");
        }
        /// <summary>
        /// Настройка слушателя подключений
        /// </summary>
        private static void LoadListener()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> Game Server Network Socket Listener Loading");

            listener = new SocketServer.ServerListener<NetworkGameServerData>(options);

#if DEBUG
            listener.OnReceivePacket += Listener_OnReceivePacket;
            listener.OnSendPacket += Listener_OnSendPacket;
#endif
            try
            {
                listener.Run();

                LoggerStorage.Instance.main.AppendInfo($"-> Game Server Network Socket Listener  ({options.IpAddress}:{options.Port}) Loaded");

            }
            catch (Exception e)
            {
                LoggerStorage.Instance.main.AppendInfo($"-> Game Server Network Socket Listener  ({options.IpAddress}:{options.Port}) Error:{e.ToString()}");
            }
        }

        #endregion

        #region Handle

#if DEBUG

        private static void Listener_OnSendPacket(Client<NetworkGameServerData> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            NetworkGameServerData c = null;
            IPEndPoint ipep = null;

            if (client != null)
            {
                c = (NetworkGameServerData)client.GetUserData();
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo( $"Game Server packet send pid:{pid}({(Info.Enums.Packets.ServerPacketsEnum)pid}) len:{len} to {ipep?.ToString()} from {sourceFilePath}:{sourceLineNumber}");
        }

        private static void Listener_OnReceivePacket(Client<NetworkGameServerData> client, ushort pid, int len)
        {
            NetworkGameServerData c = null;
            IPEndPoint ipep = null;

            if (client != null)
            {
                c = (NetworkGameServerData)client.GetUserData();
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo( $"Game Server packet receive pid:{pid}({(Info.Enums.Packets.ClientPacketsEnum)pid}) len:{len} from {ipep?.ToString()}");
        }

#endif

        /// <summary>
        /// Событие возникающее при ошибке в подключении
        /// </summary>
        /// <param name="ex">Данные об ошибке</param>
        /// <param name="s">Подключение</param>
        private static void SocketOptions_OnExtensionEvent(Exception ex, NetworkGameServerData client)
        {
            try
            {
                LoggerStorage.Instance.main.AppendError($"Game Server socket Error ({client.Network.GetSocket()?.RemoteEndPoint}) - {ex.ToString()}");
            }
            catch
            {
                LoggerStorage.Instance.main.AppendError($"Game Server socket Error  {ex.ToString()}");

            }
        }

        /// <summary>
        /// Событие возникающее при отключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private static void SocketOptions_OnClientDisconnectEvent(NetworkGameServerData client)
        {
            LoggerStorage.Instance.main.AppendInfo($"Game Server disconnected ({client?.Network?.GetRemovePoint()})");
            if(client.ServerData != null)
                {
                StaticData.GameServerManager.DisconnectServer(client.ServerData);
            }
        }

        /// <summary>
        /// Событие возникающее при подключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private static void SocketOptions_OnClientConnectEvent(NetworkGameServerData client)
        {
            LoggerStorage.Instance.main.AppendInfo($"New Game Server connection ({client?.Network?.GetRemovePoint()})");
        }

        #endregion
    }
}
