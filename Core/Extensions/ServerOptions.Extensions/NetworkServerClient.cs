using NSL.ServerOptions.Extensions.ConfigurationEngine;
using NSL.SocketClient;
using NSL.TCP.Client;
using SocketCore.Utils.Logger.Enums;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NSL.ServerOptions.Extensions
{
    public class NetworkServerClient<T, CType> : BasicNetworkEntry<T, CType, ClientOptions<T>>
        where T : BaseSocketNetworkClient
        where CType : NetworkServerClient<T, CType>
    {
        /// <summary>
        /// Слушатель подключений
        /// </summary>
        public static TCPNetworkClient<T, ClientOptions<T>> Client { get; protected set; }

        protected TCPNetworkClient<T, ClientOptions<T>> client { get; set; }

        public TCPNetworkClient<T, ClientOptions<T>> GetClient() => client;

        /// <summary>
        /// Путь к конфигурации сервера client/{ServerConfigurationName}
        /// Должен быть обзательно переопределен в случае если используеться хоть 1 функция по умолчанию
        /// </summary>
        protected virtual string ServerConfigurationName { get; } = "client";

        public virtual bool ReconnectOnDisconnected { get; } = true;

        public virtual int ReconnectDelay { get; } = 15000;

        protected virtual bool ConnectOnLoad => true;

        protected string NetworkConfigurationPath => $"client.{ServerConfigurationName}";

        public virtual void Load()
        {
            base.Load(postAction: LoadClient);
        }
        /// <summary>
        /// Инициализация и запуск сервера
        /// </summary>
        protected virtual void LoadClient()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Client Socket Client Loading");

            Client = client = new TCPNetworkClient<T, ClientOptions<T>>(options);

            client.OnReceivePacket += Listener_OnReceivePacket;
            client.OnSendPacket += Listener_OnSendPacket;
            if (!ConnectOnLoad)
                return;

            try
            {
                Connect();

                Logger?.Append(LoggerLevel.Info, $"-> Socket Client ({options.IpAddress}:{options.Port}) Loaded");
            }
            catch (Exception e)
            {
                Logger?.Append(LoggerLevel.Info, $"-> Socket Client ({options.IpAddress}:{options.Port}) Error:\r\n{e.ToString()}");
            }
        }

        protected internal override ClientOptions<T> LoadConfigurationAction()
        {
            return ConfigurationManager.LoadConfigurationCoreOptions<ClientOptions<T>, T>(NetworkConfigurationPath);
        }


        private async void Connect()
        {
            await ConnectTransport();
        }

        private async void Reconnect()
        {
            await Task.Delay(ReconnectDelay);
            await ConnectTransport();
        }

        private async Task ConnectTransport()
        {
            if (client?.GetState() != false)
                return;

            Logger?.Append(LoggerLevel.Info, $"-> {ServerName} try connect to {options.IpAddress}:{options.Port}");
            bool result = await client.ConnectAsync();

            if (result)
                Logger?.Append(LoggerLevel.Info, $"-> Success connected");
            else
                Logger?.Append(LoggerLevel.Error, $"-> Cannot connected");

        }

        #region Handle


        /// <summary>
        /// Перехват отправленных пакетов
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pid"></param>
        /// <param name="len"></param>
        /// <param name="memberName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>

        protected virtual void Listener_OnSendPacket(TCPClient<T> client, ushort pid, int len, string stackTrace)
        {
#if DEBUG
            IPEndPoint ipep = null;

            if (client != null)
            {
                ipep = client.GetRemotePoint();
            }

            Logger?.Append(LoggerLevel.Info, $"{ServerName} packet send pid:{pid} len:{len} to {ipep?.ToString()} from {stackTrace}");

#endif
        }

        /// <summary>
        /// Перехват полученных пакетов клиента
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pid"></param>
        /// <param name="len"></param>
        protected virtual void Listener_OnReceivePacket(TCPClient<T> client, ushort pid, int len)
        {
            IPEndPoint ipep = null;

            if (client != null)
            {
                ipep = client.GetRemotePoint();
            }

            Logger?.Append(LoggerLevel.Info, $"{ServerName} packet receive pid:{pid} len:{len} from {ipep?.ToString()}");
        }

        protected override void SocketOptions_OnClientDisconnectEvent(T client)
        {
            base.SocketOptions_OnClientDisconnectEvent(client);
            if (ReconnectOnDisconnected)
                Reconnect();
        }

        #endregion
    }
}
