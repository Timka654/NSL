using Cipher.RC.RC4;
using SocketServer;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.NodeHostServer.Packets;
using System.Net;
using phs.Data.GameServer.Managers;
using Utils.Logger;
using Utils.Helper.Network;
using Cipher;
using Utils.Helper;
using phs.Data.NodeHostServer.Managers;

namespace phs.Data.NodeHostServer.Network
{
    /// <summary>
    /// Сервер приема клиентов
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Глобальные настройки сервера
        /// </summary>
        public static ServerOptions<NetworkNodeServerData> options;

        /// <summary>
        /// Слушатель подключений
        /// </summary>
        public static ServerListener<NetworkNodeServerData> listener;

        #region Load

        /// <summary>
        /// Полная инициализация
        /// </summary>
        public static void Load()
        {
            LoggerStorage.Instance.main.AppendInfo($"---> Client Server Loading");

            LoadConfiguration();

            LoadManagers();

            Structorian.BuildStructures(BinarySerializer.TypeStorage.Instance);

            LoadReceivePackets();

            LoadListener();

            LoggerStorage.Instance.main.AppendInfo($"---> Client Server Loaded");
        }

        /// <summary>
        /// Загрузка конфигурации, установка handle
        /// </summary>
        private static void LoadConfiguration()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> Configuration Loading");

            options = StaticData.ConfigurationManager.LoadConfigurationServerOptions<NetworkNodeServerData>("network/node_host_server");

            options.inputCipher = new PacketNoneCipher();
            options.outputCipher = new PacketNoneCipher();


            options.OnClientConnectEvent += SocketOptions_OnClientConnectEvent;
            options.OnClientDisconnectEvent += SocketOptions_OnClientDisconnectEvent;
            options.OnExtensionEvent += SocketOptions_OnExtensionEvent;

            LoggerStorage.Instance.main.AppendInfo($"-> Configuration Loaded");
        }

        /// <summary>
        /// Загрузка менеджеров(Обработчиков)
        /// </summary>
        private static void LoadManagers()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> Managers Loading");

            options.LoadManagers(typeof(NodeHostManagerLoadAttribute));

            LoggerStorage.Instance.main.AppendInfo($"-> Managers Loaded");
        }

        /// <summary>
        /// Загрузка пакетов для приема с клиента
        /// </summary>
        private static void LoadReceivePackets()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> Client Server Packets Loading");

            options.LoadPackets();

            LoggerStorage.Instance.main.AppendInfo($"-> Client Server Packets Loaded");
        }

        /// <summary>
        /// Настройка слушателя подключений
        /// </summary>
        private static void LoadListener()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> Client Socket Listener Loading");

            listener = new SocketServer.ServerListener<NetworkNodeServerData>(options);

#if DEBUG
            listener.OnReceivePacket += Listener_OnReceivePacket;
            listener.OnSendPacket += Listener_OnSendPacket;
#endif
            try
            {
                listener.Run();

                LoggerStorage.Instance.main.AppendInfo($"-> Client Socket Listener  ({options.IpAddress}:{options.Port}) Loaded");

            }
            catch (Exception e)
            {
                LoggerStorage.Instance.main.AppendInfo($"-> Client Socket Listener  ({options.IpAddress}:{options.Port}) Error:{e.ToString()}");
            }
        }

        #endregion

        #region Handle

#if DEBUG

        private static void Listener_OnSendPacket(Client<NetworkNodeServerData> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            NetworkNodeServerData c = null;
            IPEndPoint ipep = null;

            if (client != null)
            {
                c = (NetworkNodeServerData)client.GetUserData();
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo($"Client packet send pid:{pid}({(Info.Enums.Packets.ClientPacketsEnum)pid}) len:{len} to {ipep?.ToString()} ({c?.ServerInfo?.Id}) from {sourceFilePath}:{sourceLineNumber}");
        }

        private static void Listener_OnReceivePacket(Client<NetworkNodeServerData> client, ushort pid, int len)
        {
            NetworkNodeServerData c = null;
            IPEndPoint ipep = null;

            if (client != null)
            {
                c = (NetworkNodeServerData)client.GetUserData();
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo($"Client packet receive pid:{pid}({(Info.Enums.Packets.ServerPacketsEnum)pid}) len:{len} from {ipep?.ToString()} ({c?.ServerInfo?.Id})");
        }

#endif

        /// <summary>
        /// Событие возникающее при ошибке в подключении
        /// </summary>
        /// <param name="ex">Данные об ошибке</param>
        /// <param name="s">Подключение</param>
        private static void SocketOptions_OnExtensionEvent(Exception ex, NetworkNodeServerData client)
        {
            try
            {
                LoggerStorage.Instance.main.AppendError($"Client socket Error ({client.Network.GetSocket()?.RemoteEndPoint}) - {ex.ToString()}");
            }
            catch
            {
                LoggerStorage.Instance.main.AppendError($"Client socket Error  {ex.ToString()}");

            }
        }

        /// <summary>
        /// Событие возникающее при отключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private static void SocketOptions_OnClientDisconnectEvent(NetworkNodeServerData client)
        {
            LoggerStorage.Instance.main.AppendInfo($"Client disconnected ({client?.Network?.GetRemovePoint()})");
            if (client != null)
            {
                client.DisconnectTime = DateTime.Now;
                if (client.ServerInfo != null)
                    StaticData.ProxyServerManager.DisconnectServer(client.ServerInfo);
            }
        }

        /// <summary>
        /// Событие возникающее при подключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private static void SocketOptions_OnClientConnectEvent(NetworkNodeServerData client)
        {
            LoggerStorage.Instance.main.AppendInfo($"New client connection ({client?.Network?.GetRemovePoint()})");
        }

        #endregion
    }
}
