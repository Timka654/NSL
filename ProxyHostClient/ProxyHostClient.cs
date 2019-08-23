using Cipher;
using SocketServer;
using System;
using ProxyHostClient.Packets;
using System.Net;
using ProxyHostClient.Managers;
using Utils.Logger;
using Utils.Helper.Network;
using SocketServer.Utils;
using System.Threading.Tasks;
using Utils.Helper;
using Utils.Helper.Configuration;

namespace ProxyHostClient
{
    public class ProxyHostClient
    {
        /// <summary>
        /// Глобальные настройки сервера
        /// </summary>
        public static ServerOptions<ProxyHostClientData> options;

        public static NetworkClient<ProxyHostClientData> NetworkClient;

        internal static ConfigurationManager Config;

        private static string NetworkNodePath;

        #region Load

        /// <summary>
        /// Полная инициализация
        /// </summary>
        public static void Load(ConfigurationManager config, string networkNodePath)
        {
            Config = config;

            NetworkNodePath = networkNodePath;

            LoggerStorage.Instance.main.AppendInfo($"---> Node Host Client Loading");

            LoadConfiguration();

            LoadManagers();

            LoadReceivePackets();

            LoadClient();

            LoggerStorage.Instance.main.AppendInfo($"---> Node Host Client Loaded");
        }

        /// <summary>
        /// Загрузка конфигурации, установка handle
        /// </summary>
        private static void LoadConfiguration()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> Configuration Loading");

            options = Config.LoadConfigurationServerOptions<ProxyHostClientData>("network/node_host_client");

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

            new NodePlayerManager();

            LoggerStorage.Instance.main.AppendInfo($"-> Managers Loaded");
        }

        /// <summary>
        /// Загрузка пакетов для приема с клиента
        /// </summary>
        private static void LoadReceivePackets()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> Node Host Client Packets Loading");

            options.LoadPackets(typeof(ProxyHostPacketAttribute));

            LoggerStorage.Instance.main.AppendInfo($"-> Node Host Client Packets Loaded");
        }

        /// <summary>
        /// Настройка слушателя подключений
        /// </summary>
        private static void LoadClient()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> Node Host Socket Listener Loading");

            NetworkClient = new NetworkClient<ProxyHostClientData>(options);

#if DEBUG
            NetworkClient.OnReceivePacket += Listener_OnReceivePacket;
            NetworkClient.OnSendPacket += Listener_OnSendPacket;
#endif
            Connect();
        }

        private static async void Connect()
        {
            while (!NetworkClient.Connect())
            {
                LoggerStorage.Instance.main.AppendInfo($"-> Node Host client ({options.IpAddress}:{options.Port}) Error connection");
                await System.Threading.Tasks.Task.Delay(2000);
            }
        }

        #endregion

        #region Handle

#if DEBUG

        private static void Listener_OnSendPacket(Client<ProxyHostClientData> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            ProxyHostClientData c = null;
            IPEndPoint ipep = null;

            if (client != null)
            {
                c = (ProxyHostClientData)client.GetUserData();
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo($"Node Host packet send pid:{pid}({(Packets.Enums.ServerPacketsEnum)pid}) len:{len} to {ipep?.ToString()} from {sourceFilePath}:{sourceLineNumber}");
        }

        private static void Listener_OnReceivePacket(Client<ProxyHostClientData> client, ushort pid, int len)
        {
            ProxyHostClientData c = null;
            IPEndPoint ipep = null;

            if (client != null)
            {
                c = (ProxyHostClientData)client.GetUserData();
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo($"Node Host packet receive pid:{pid}({(Packets.Enums.ClientPacketsEnum)pid}) len:{len} from {ipep?.ToString()}");
        }

#endif

        /// <summary>
        /// Событие возникающее при ошибке в подключении
        /// </summary>
        /// <param name="ex">Данные об ошибке</param>
        /// <param name="client">Текущий клиент</param>
        private static async void SocketOptions_OnExtensionEvent(Exception ex, ProxyHostClientData client)
        {
            try
            {
                LoggerStorage.Instance.main.AppendError($"Node Host socket Error ({client?.Network?.GetSocket()?.RemoteEndPoint}) - {ex.ToString()}");

            }
            catch
            {
                LoggerStorage.Instance.main.AppendError($"Node Host socket Error  {ex.ToString()}");

            }
            await Task.Delay(5000);
            Connect();
        }

        /// <summary>
        /// Событие возникающее при отключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private static void SocketOptions_OnClientDisconnectEvent(ProxyHostClientData client)
        {
            LoggerStorage.Instance.main.AppendInfo($"Node Host network client disconnected ({client?.Network?.GetRemovePoint()})");
        }

        /// <summary>
        /// Событие возникающее при подключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private static void SocketOptions_OnClientConnectEvent(ProxyHostClientData client)
        {
            LoggerStorage.Instance.main.AppendInfo($"Node Host network client connected ({client?.Network?.GetRemovePoint()})");
        }

        #endregion

        #region UserMethods

        public static void SignIn(short serverId, string password)
        {
            Packets.Auth.SignIn.Send((ProxyHostClientData)NetworkClient.GetUserData(),serverId, password);
        }

        public static void ConnectionResult(Guid guid, bool result)
        {
            Packets.Player.PlayerConnected.Send((ProxyHostClientData)NetworkClient.GetUserData(), guid, result);
        }

        public static void DisconnectPlayer(Guid guid)
        {
            Packets.Player.PlayerDisconnected.Send((ProxyHostClientData)NetworkClient.GetUserData(), guid);
        }

        #endregion
    }
}
