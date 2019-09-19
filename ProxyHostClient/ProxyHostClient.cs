using Cipher;
using SocketServer;
using System;
using ProxyHostClient.Packets;
using System.Net;
using Utils.Helper.Network;
using SocketServer.Utils;
using System.Threading.Tasks;
using Utils.Helper;
using Utils.Helper.Configuration;
using ProxyHostClient.Packets.Player.PacketData;
using ProxyHostClient.Packets.Auth;
using Logger;

namespace ProxyHostClient
{
    public class ProxyHostClient
    {
        public delegate void OnPlayerConnectedDelegate(PlayerConnectedPacketData player);
        public delegate void OnPlayerDisconnectedDelegate(Guid id);


        /// <summary>
        /// Глобальные настройки сервера
        /// </summary>
        public ServerOptions<ProxyHostClientData> options;

        public NetworkClient<ProxyHostClientData> NetworkClient;

        internal ConfigurationManager Config;

        private string NetworkNodePath;

        private string PublicName;

        #region Events

        public event EventHandler<bool> OnSignInResult
        {
            add { ((Packets.Auth.SignIn)options.GetPacket((ushort)Packets.Enums.ClientPacketsEnum.SignInResult)).OnReceive += value; }
            remove { ((Packets.Auth.SignIn)options.GetPacket((ushort)Packets.Enums.ClientPacketsEnum.SignInResult)).OnReceive -= value; }
        }

        public event OnPlayerConnectedDelegate OnPlayerConnected {
            add { ((Packets.Player.PlayerConnected)options.GetPacket((ushort)Packets.Enums.ClientPacketsEnum.PlayerConnected)).OnReceive += value; }
            remove { ((Packets.Player.PlayerConnected)options.GetPacket((ushort)Packets.Enums.ClientPacketsEnum.PlayerConnected)).OnReceive -= value; }
        }

        public event OnPlayerDisconnectedDelegate OnPlayerDisconnected
        {
            add { ((Packets.Player.PlayerDisconnected)options.GetPacket((ushort)Packets.Enums.ClientPacketsEnum.PlayerDisconnected)).OnReceive += value; }
            remove { ((Packets.Player.PlayerDisconnected)options.GetPacket((ushort)Packets.Enums.ClientPacketsEnum.PlayerDisconnected)).OnReceive -= value; }
        }

        public event GetProxyServerHandle OnProxyServer
        {
            add { ((Packets.Auth.GetProxyServer)options.GetPacket((ushort)Packets.Enums.ClientPacketsEnum.GetProxyServerResult)).OnReceive += value; }
            remove { ((Packets.Auth.GetProxyServer)options.GetPacket((ushort)Packets.Enums.ClientPacketsEnum.GetProxyServerResult)).OnReceive -= value; }
        }

        #endregion

        #region Load

        /// <summary>
        /// Полная инициализация
        /// </summary>
        public void Load(ConfigurationManager config, string networkNodePath, string publicName = "Node Host")
        {
            Config = config;

            NetworkNodePath = networkNodePath;
            PublicName = publicName;

            LoggerStorage.Instance.main.AppendInfo($"---> {PublicName} Client Loading");

            LoadConfiguration();

            LoadReceivePackets();

            LoadClient();

            LoggerStorage.Instance.main.AppendInfo($"---> {PublicName} Client Loaded");
        }

        /// <summary>
        /// Загрузка конфигурации, установка handle
        /// </summary>
        private void LoadConfiguration()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> Configuration Loading");

            options = Config.LoadConfigurationServerOptions<ProxyHostClientData>(NetworkNodePath);

            options.inputCipher = new PacketNoneCipher();
            options.outputCipher = new PacketNoneCipher();

            options.OnClientConnectEvent += SocketOptions_OnClientConnectEvent;
            options.OnClientDisconnectEvent += SocketOptions_OnClientDisconnectEvent;
            options.OnExtensionEvent += SocketOptions_OnExtensionEvent;

            LoggerStorage.Instance.main.AppendInfo($"-> Configuration Loaded");
        }

        /// <summary>
        /// Загрузка пакетов для приема с клиента
        /// </summary>
        private void LoadReceivePackets()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> {PublicName} Client Packets Loading");

            options.LoadPackets(typeof(ProxyHostPacketAttribute));

            LoggerStorage.Instance.main.AppendInfo($"-> {PublicName} Client Packets Loaded");
        }

        /// <summary>
        /// Настройка слушателя подключений
        /// </summary>
        private void LoadClient()
        {
            LoggerStorage.Instance.main.AppendInfo($"-> {PublicName} Socket Listener Loading");

            NetworkClient = new NetworkClient<ProxyHostClientData>(options);

#if DEBUG
            NetworkClient.OnReceivePacket += Listener_OnReceivePacket;
            NetworkClient.OnSendPacket += Listener_OnSendPacket;
#endif
        }

        public bool Connect()
        {
            bool result = NetworkClient.Connect();
            if (!result)
                LoggerStorage.Instance.main.AppendError($"-> {PublicName} client ({options.IpAddress}:{options.Port}) Error connection");
            else
                SignIn(Config.GetValue<int>($"{NetworkNodePath}/signin/server.id"), Config.GetValue<string>($"{NetworkNodePath}/signin/access.key"));

            return result;
        }

        #endregion

        #region Handle

#if DEBUG

        private void Listener_OnSendPacket(Client<ProxyHostClientData> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            IPEndPoint ipep = null;

            if (client != null)
            {
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo($"{PublicName} packet send pid:{pid}({(Packets.Enums.ServerPacketsEnum)pid}) len:{len} to {ipep?.ToString()} from {sourceFilePath}:{sourceLineNumber}");
        }

        private void Listener_OnReceivePacket(Client<ProxyHostClientData> client, ushort pid, int len)
        {
            IPEndPoint ipep = null;

            if (client != null)
            {
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo($"{PublicName} packet receive pid:{pid}({(Packets.Enums.ClientPacketsEnum)pid}) len:{len} from {ipep?.ToString()}");
        }

#endif

        /// <summary>
        /// Событие возникающее при ошибке в подключении
        /// </summary>
        /// <param name="ex">Данные об ошибке</param>
        /// <param name="client">Текущий клиент</param>
        private async void SocketOptions_OnExtensionEvent(Exception ex, ProxyHostClientData client)
        {
            try
            {
                LoggerStorage.Instance.main.AppendError($"{PublicName} socket Error ({client?.Network?.GetSocket()?.RemoteEndPoint}) - {ex.ToString()}");

            }
            catch
            {
                LoggerStorage.Instance.main.AppendError($"{PublicName} socket Error  {ex.ToString()}");

            }
            await Task.Delay(5000);
            Connect();
        }

        /// <summary>
        /// Событие возникающее при отключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private void SocketOptions_OnClientDisconnectEvent(ProxyHostClientData client)
        {
            LoggerStorage.Instance.main.AppendInfo($"{PublicName} network client disconnected ({client?.Network?.GetRemovePoint()})");
        }

        /// <summary>
        /// Событие возникающее при подключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private void SocketOptions_OnClientConnectEvent(ProxyHostClientData client)
        {
            LoggerStorage.Instance.main.AppendInfo($"{PublicName} network client connected ({client?.Network?.GetRemovePoint()})");
        }

        #endregion

        #region UserMethods

        public void SignIn(int serverId, string password)
        {
            Packets.Auth.SignIn.Send((ProxyHostClientData)NetworkClient.GetUserData(), serverId, password);
        }

        public void ConnectionResult(Guid guid, bool result)
        {
            Packets.Player.PlayerConnected.Send((ProxyHostClientData)NetworkClient.GetUserData(), guid, result);
        }

        public void DisconnectPlayer(Guid guid)
        {
            Packets.Player.PlayerDisconnected.Send((ProxyHostClientData)NetworkClient.GetUserData(), guid);
        }

        public void GetRoomProxyServer(int roomId)
        {
            Packets.Auth.GetProxyServer.Send((ProxyHostClientData)NetworkClient.GetUserData(), roomId);
        }

        #endregion
    }
}
