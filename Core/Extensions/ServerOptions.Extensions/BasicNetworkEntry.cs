using Cipher;
using ConfigurationEngine;
using SCLogger;
using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.Cipher;
using SocketCore.Utils.Logger.Enums;
using System;

namespace Network.Extensions
{
    public class BasicNetworkEntry<T, CType, OType>
        where T : INetworkClient
        where CType : BasicNetworkEntry<T, CType, OType>
        where OType : CoreOptions<T>, new()
    {
        /// <summary>
        /// Инстанс singleton обьекта
        /// По умолчанию всегда инициализирован
        /// </summary>
        public static CType Instance { get; private set; } = Activator.CreateInstance<CType>();

        /// <summary>
        /// Глобальные настройки сервера
        /// </summary>
        public static OType Options { get; protected set; }

        protected OType options { get; set; }

        public OType GetOptions() => options;

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

        internal protected string LowerCaseServerName => ServerName.ToLower();

        /// <summary>
        /// Менеджер конфигураций для установки значений в функциях по умолчанию
        /// Должен быть обзательно переопределен в случае если используеться хоть 1 функция по умолчанию
        /// </summary>
        protected virtual IConfigurationManager ConfigurationManager { get; }

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
        protected internal void Load(Action preAction = null, Action postAction = null)
        {
            Logger?.Append(LoggerLevel.Info, $"---> {ServerName} server Loading");

            preAction?.Invoke();

            LoadConfiguration();
            LoadManagers();
            LoadStructorian();
            LoadReceivePackets();

            postAction?.Invoke();
            //LoadListener();

            Logger?.Append(LoggerLevel.Info, $"---> {ServerName} server Loaded");
        }

        protected internal virtual OType LoadConfigurationAction()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Инициализация переменной options
        /// Может быть использовано для установки других значений
        /// </summary>
        protected virtual void LoadConfiguration()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Configuration Loading");

            options = Options = LoadConfigurationAction();

            options.HelperLogger = Logger;

            if (options.inputCipher == null)
                options.inputCipher = new PacketNoneCipher();
            if (options.outputCipher == null)
                options.outputCipher = new PacketNoneCipher();

            options.OnClientConnectEvent += SocketOptions_OnClientConnectEvent;
            options.OnClientDisconnectEvent += SocketOptions_OnClientDisconnectEvent;
            options.OnExceptionEvent += SocketOptions_OnExtensionEvent;

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

            LoadManagersAction();

            Logger?.Append(LoggerLevel.Info, $"-> Managers Loaded");
        }

        protected virtual void LoadManagersAction()
        {

        }

        /// <summary>
        /// Построение структур данных при помощи <see cref="BinarySerializer.Builder.StructBuilder"/>
        /// По умолчанию функция ничего не делает кроме вывода сообщений о начале и окончанию инициализации
        /// </summary>
        protected virtual void LoadStructorian()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Structorian Loading");

            LoadStructorianAction();

            Logger?.Append(LoggerLevel.Info, $"-> Structorian Loadeded");
        }

        protected virtual void LoadStructorianAction()
        {

        }

        /// <summary>
        /// Инициализация принимаемых пакетов связанных с этим сервером
        /// По умолчанию функция ничего не делает кроме вывода сообщений о начале и окончанию инициализации
        /// <seealso cref="PacketHelper.LoadPackets{T}(ServerOptions{T}, Type)"/>
        /// </summary>
        protected virtual void LoadReceivePackets()
        {
            Logger?.Append(LoggerLevel.Info, $"-> Packets Loading");

            LoadReceivePacketsAction();

            Logger?.Append(LoggerLevel.Info, $"-> Packets Loaded");
        }

        protected virtual void LoadReceivePacketsAction()
        {

        }


        #endregion

        #region Handle

        /// <summary>
        /// Перехват ошибок обработки клиента
        /// </summary>
        /// <param name="ex">Данные об ошибке</param>
        /// <param name="s">Подключение</param> 
        protected virtual void SocketOptions_OnExtensionEvent(Exception ex, T client)
        {
            try
            {
                Logger?.Append(LoggerLevel.Error, $"{ServerName} socket Error ({client?.Network?.GetSocket()?.RemoteEndPoint}) - {ex.ToString()}");
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
            Logger?.Append(LoggerLevel.Info, $"{ServerName} disconnected ({client?.Network?.GetRemotePoint()})");
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
            Logger?.Append(LoggerLevel.Info, $"New {LowerCaseServerName} connection ({client?.Network?.GetRemotePoint()})");
        }

        #endregion
    }
}
