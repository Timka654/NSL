using Cipher;
using SocketServer;
using System;
using System.Collections.Generic;
using System.Text;
using ps.Data.NodeServer.Network;
using ps.Data.NodeHostClient.Packets;
using System.Net;
using System.Net.Sockets;
using ps.Data.NodeHostClient.Info;
using ps.Data.NodeHostClient.Info.Enums;
using ps.Data.NodeHostClient.Managers;
using Utils.Logger;
using Utils.Helper.Network;
using SocketServer.Utils;

namespace ps.Data.NodeHostClient.Network
{
    /// <summary>
    /// Клиент приема подключений Node Host серверов
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Глобальные настройки сервера
        /// </summary>
        public static ServerOptions<NetworkNodeHostClientData> options;

        public static NetworkClient<NetworkNodeHostClientData> NetworkClient;

        #region Load

        /// <summary>
        /// Полная инициализация
        /// </summary>
        public static void Load()
        {
            LoggerStorage.Instance.main.AppendInfo( $"---> Node Host Client Loading");

            LoadConfiguration();

            LoadManagers();

            Structorian.BuildStructures(BinarySerializer.TypeStorage.Instance);

            LoadReceivePackets();

            LoadClient();

            LoggerStorage.Instance.main.AppendInfo( $"---> Node Host Client Loaded");
        }

        /// <summary>
        /// Загрузка конфигурации, установка handle
        /// </summary>
        private static void LoadConfiguration()
        {
            LoggerStorage.Instance.main.AppendInfo( $"-> Configuration Loading");

            options = StaticData.ConfigurationManager.LoadConfigurationServerOptions<NetworkNodeHostClientData>("network/node_host_client");

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

            new NodePlayerManager();

            LoggerStorage.Instance.main.AppendInfo( $"-> Managers Loaded");
        }

        /// <summary>
        /// Загрузка пакетов для приема с клиента
        /// </summary>
        private static void LoadReceivePackets()
        {
            LoggerStorage.Instance.main.AppendInfo( $"-> Node Host Client Packets Loading");

            options.LoadPackets();

            LoggerStorage.Instance.main.AppendInfo( $"-> Node Host Client Packets Loaded");
        }

        /// <summary>
        /// Настройка слушателя подключений
        /// </summary>
        private static void LoadClient()
        {
            LoggerStorage.Instance.main.AppendInfo( $"-> Node Host Socket Listener Loading");

            NetworkClient = new NetworkClient<NetworkNodeHostClientData>(options);

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

        private static void Listener_OnSendPacket(Client<NetworkNodeHostClientData> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            NetworkNodeHostClientData c = null;
            IPEndPoint ipep = null;

            if (client != null)
            {
                c = (NetworkNodeHostClientData)client.GetUserData();
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo( $"Node Host packet send pid:{pid}({(Info.Enums.Packets.ServerPacketsEnum)pid}) len:{len} to {ipep?.ToString()} from {sourceFilePath}:{sourceLineNumber}");
        }

        private static void Listener_OnReceivePacket(Client<NetworkNodeHostClientData> client, ushort pid, int len)
        {
            NetworkNodeHostClientData c = null;
            IPEndPoint ipep = null;

            if (client != null)
            {
                c = (NetworkNodeHostClientData)client.GetUserData();
                ipep = client.GetRemovePoint();
            }

            LoggerStorage.Instance.main.AppendInfo( $"Node Host packet receive pid:{pid}({(Info.Enums.Packets.ClientPacketsEnum)pid}) len:{len} from {ipep?.ToString()}");
        }

#endif

        /// <summary>
        /// Событие возникающее при ошибке в подключении
        /// </summary>
        /// <param name="ex">Данные об ошибке</param>
        /// <param name="client">Текущий клиент</param>
        private static void SocketOptions_OnExtensionEvent(Exception ex, NetworkNodeHostClientData client)
        {
            try
            {
            LoggerStorage.Instance.main.AppendError( $"Node Host socket Error ({client?.Network?.GetSocket()?.RemoteEndPoint}) - {ex.ToString()}");

            }
            catch
            {
            LoggerStorage.Instance.main.AppendError( $"Node Host socket Error  {ex.ToString()}");

            }
        }

        /// <summary>
        /// Событие возникающее при отключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private static void SocketOptions_OnClientDisconnectEvent(NetworkNodeHostClientData client)
        {
            LoggerStorage.Instance.main.AppendInfo( $"Node Host network client disconnected ({client?.Network?.GetRemovePoint()})");
        }

        /// <summary>
        /// Событие возникающее при подключении клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        private static void SocketOptions_OnClientConnectEvent(NetworkNodeHostClientData client)
        {
            LoggerStorage.Instance.main.AppendInfo($"Node Host network client connected ({client?.Network?.GetRemovePoint()})");
            Packets.Auth.SignIn.Send();
        }

        #endregion
    }
}
