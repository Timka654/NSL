using Network.Extensions;
using ServerOptions.Extensions.ConfigurationEngine;
using SocketCore.Utils.Logger.Enums;
using SocketServer;
using SocketServer.Utils;
using System;
using System.Net;

namespace Utils.Helper.Network
{
    public class NetworkServer<T, CType> : BasicNetworkEntry<T, CType,ServerOptions<T>>
        where T : IServerNetworkClient
        where CType : NetworkServer<T,CType>
    {

        /// <summary>
        /// Слушатель подключений
        /// </summary>
        public static ServerListener<T> Listener { get; protected set; }

        /// <summary>
        /// Путь к конфигурации сервера server/{ServerConfigurationName}
        /// Должен быть обзательно переопределен в случае если используеться хоть 1 функция по умолчанию
        /// </summary>
        protected virtual string ServerConfigurationName { get; } = "client";

        public virtual void Load()
        {
            base.Load(postAction: ()=> { LoadListener(); });

        }
        /// <summary>
        /// Инициализация и запуск сервера
        /// </summary>
        protected virtual void LoadListener()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Client Socket Listener Loading");

            Listener = new SocketServer.ServerListener<T>(Options);

            Listener.OnReceivePacket += Listener_OnReceivePacket;
            Listener.OnSendPacket += Listener_OnSendPacket;

            try
            {
                Listener.Run();

                Logger?.Append(LoggerLevel.Info, $"-> Socket Listener ({Options.IpAddress}:{Options.Port}) Loaded");
            }
            catch (Exception e)
            {
                Logger?.Append(LoggerLevel.Info, $"-> Socket Listener ({Options.IpAddress}:{Options.Port}) Error:\r\n{e.ToString()}");
            }
        }

        protected internal override ServerOptions<T> LoadConfigurationAction()
        {
            return ConfigurationManager.LoadConfigurationServerOptions<T>($"server.{ServerConfigurationName}");
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
#if DEBUG
        protected virtual void Listener_OnSendPacket(ServerClient<T> client, ushort pid, int len, string stacktrace)
        {
            IPEndPoint ipep = null;

            if (client != null)
            {
                ipep = client.GetRemovePoint();
            }

            Logger?.Append(LoggerLevel.Info, $"{ServerName} packet send pid:{pid} len:{len} to {ipep?.ToString()} from {stacktrace}");
        }
#else
        protected virtual void Listener_OnSendPacket(ServerClient<T> client, ushort pid, int len)
        { 
        
        }
#endif

        /// <summary>
        /// Перехват полученных пакетов клиента
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pid"></param>
        /// <param name="len"></param>
        protected virtual void Listener_OnReceivePacket(ServerClient<T> client, ushort pid, int len)
        {
            IPEndPoint ipep = null;

            if (client != null)
            {
                ipep = client.GetRemovePoint();
            }

            Logger?.Append(LoggerLevel.Info, $"{ServerName} packet receive pid:{pid} len:{len} from {ipep?.ToString()}");
        }

#endregion
    }
}
