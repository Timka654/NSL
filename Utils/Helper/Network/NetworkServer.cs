using Cipher;
using Logger;
using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Utils.Helper.Configuration;

namespace Utils.Helper.Network
{
    public class NetworkServer<T, CType>
        where T : INetworkClient
        where CType : NetworkServer<T,CType>
    {
        /// <summary>
        /// Инстанс singleton обьекта
        /// По умолчанию всегда инициализирован
        /// </summary>
        public static CType Instance { get; private set; } = Activator.CreateInstance<CType>();

        /// <summary>
        /// Глобальные настройки сервера
        /// </summary>
        public static ServerOptions<T> Options { get; protected set; }

        /// <summary>
        /// Слушатель подключений
        /// </summary>
        public static ServerListener<T> Listener { get; protected set; }

        /// <summary>
        /// Логгер используемый в функциях по умолчанию
        /// Должен быть обзательно переопределен в случае если используеться хоть 1 функция по умолчанию
        /// </summary>
        protected virtual ILogger Logger { get; }

        /// <summary>
        /// Название сервера отображаемое в функциях по умолчанию
        /// Должен быть обзательно переопределен в случае если используеться хоть 1 функция по умолчанию
        /// </summary>
        protected virtual string ServerName { get; } = "Client";

        /// <summary>
        /// Путь к конфигурации сервера server/{ServerConfigurationName}
        /// Должен быть обзательно переопределен в случае если используеться хоть 1 функция по умолчанию
        /// </summary>
        protected virtual string ServerConfigurationName { get; } = "client";

        private string LowerCaseServerName => ServerName.ToLower();

        /// <summary>
        /// Менеджер конфигураций для установки значений в функциях по умолчанию
        /// Должен быть обзательно переопределен в случае если используеться хоть 1 функция по умолчанию
        /// </summary>
        protected virtual ConfigurationManager ConfigurationManager { get; }

        #region Loading

        /// <summary>
        /// Полная инициализация
        /// Вызов функций в порядке
        /// <see cref="LoadConfiguration">LoadConfiguration</see>
        /// <see cref="LoadManagers"> LoadManagers</see>
        /// <see cref="LoadStructorian">LoadStructorian</see>
        /// <see cref="LoadReceivePackets"> LoadReceivePackets</see>
        /// <see cref="LoadListener"> LoadListener</see>
        /// </summary>
        public virtual void Load()
        {
            Logger?.Append(LoggerLevel.Info, $"---> {ServerName} server Loading");

            LoadConfiguration();
            LoadManagers();
            LoadStructorian();
            LoadReceivePackets();
            LoadListener();

            Logger?.Append(LoggerLevel.Info, $"---> {ServerName} server Loaded");
        }

        /// <summary>
        /// Инициализация переменной Options
        /// Может быть использовано для установки других значений
        /// </summary>
        protected virtual void LoadConfiguration()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Configuration Loading");

            Options = ConfigurationManager.LoadConfigurationServerOptions<T>($"server/{ServerConfigurationName}");

            Options.HelperLogger = Logger;

            Options.inputCipher = new PacketNoneCipher();
            Options.outputCipher = new PacketNoneCipher();

            Options.OnClientConnectEvent += SocketOptions_OnClientConnectEvent;
            Options.OnClientDisconnectEvent += SocketOptions_OnClientDisconnectEvent;
            Options.OnExtensionEvent += SocketOptions_OnExtensionEvent;

            Logger?.Append(LoggerLevel.Info, $"-> Configuration Loaded");
        }

        /// <summary>
        /// Инициализация менеджеров связанных с этим сервером 
        /// По умолчанию функция ничего не делает кроме вывода сообщений о начале и окончанию инициализации
        /// Для быстрой инициализации используйте <seealso cref="Helper.ManagerHelper.LoadManagers{T}(ServerOptions{T}, Type)"/>
        /// </summary>
        protected virtual void LoadManagers()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Managers Loading");

            Logger?.Append(LoggerLevel.Info, $"-> Managers Loaded");
        }

        /// <summary>
        /// Построение структур данных при помощи <see cref="BinarySerializer.Builder.StructBuilder"/>
        /// По умолчанию функция ничего не делает кроме вывода сообщений о начале и окончанию инициализации
        /// </summary>
        protected virtual void LoadStructorian()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Structorian Loading");

            Logger?.Append(LoggerLevel.Info, $"-> Structorian Loadeded");
        }

        /// <summary>
        /// Инициализация принимаемых пакетов связанных с этим сервером
        /// По умолчанию функция ничего не делает кроме вывода сообщений о начале и окончанию инициализации
        /// <seealso cref="PacketHelper.LoadPackets{T}(ServerOptions{T}, Type)"/>
        /// </summary>
        protected virtual void LoadReceivePackets()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Packets Loading");

            Logger?.Append(LoggerLevel.Info, $"-> Packets Loaded");
        }

        /// <summary>
        /// Инициализация и запуск сервера
        /// </summary>
        protected virtual void LoadListener()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Client Socket Listener Loading");

            Listener = new SocketServer.ServerListener<T>(Options);

#if DEBUG
            Listener.OnReceivePacket += Listener_OnReceivePacket;
            Listener.OnSendPacket += Listener_OnSendPacket;
#endif

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

        #endregion

        #region Handle

#if DEBUG

        /// <summary>
        /// Перехват отправленных пакетов (только в режиме отладки)
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pid"></param>
        /// <param name="len"></param>
        /// <param name="memberName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        protected virtual void Listener_OnSendPacket(Client<T> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            IPEndPoint ipep = null;

            if (client != null)
            {
                ipep = client.GetRemovePoint();
            }

            Logger?.Append(LoggerLevel.Info, $"{ServerName} packet send pid:{pid} len:{len} to {ipep?.ToString()} from {sourceFilePath}:{sourceLineNumber}");
        }

        /// <summary>
        /// Перехват полученных пакетов клиента (только в режиме отладки)
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pid"></param>
        /// <param name="len"></param>
        protected virtual void Listener_OnReceivePacket(Client<T> client, ushort pid, int len)
        {
            IPEndPoint ipep = null;

            if (client != null)
            {
                ipep = client.GetRemovePoint();
            }

            Logger?.Append(LoggerLevel.Info, $"{ServerName} packet receive pid:{pid} len:{len} from {ipep?.ToString()}");
        }

#endif

        /// <summary>
        /// Перехват ошибок обработки клиента
        /// </summary>
        /// <param name="ex">Данные об ошибке</param>
        /// <param name="s">Подключение</param>
        protected virtual void SocketOptions_OnExtensionEvent(Exception ex, T client)
        {
            try
            {
                Logger?.Append(LoggerLevel.Error, $"{ServerName} socket Error ({client.Network.GetSocket()?.RemoteEndPoint}) - {ex.ToString()}");
            }
            catch
            {
                Logger?.Append(LoggerLevel.Error, $"{ServerName} socket Error  {ex.ToString()}");
            }
        }

        /// <summary>
        /// Перехват отключения клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        protected virtual void SocketOptions_OnClientDisconnectEvent(T client)
        {
            Logger?.Append(LoggerLevel.Info, $"{ServerName} disconnected ({client?.Network?.GetRemovePoint()})");
            if (client != null)
            {
                client.DisconnectTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Перехват подключения клиента
        /// </summary>
        /// <param name="client">Текущий клиент</param>
        protected virtual void SocketOptions_OnClientConnectEvent(T client)
        {
            Logger?.Append(LoggerLevel.Info, $"New {LowerCaseServerName} connection ({client?.Network?.GetRemovePoint()})");
        }

        #endregion
    }
}
