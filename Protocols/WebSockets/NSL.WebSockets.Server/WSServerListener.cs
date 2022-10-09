using NSL.SocketCore;
using NSL.SocketServer.Utils;
using NSL.SocketServer;
using System;
using System.Net;

namespace NSL.WebSockets.Server
{
    public class WSServerListener<T> : INetworkListener<T>
        where T : IServerNetworkClient, new()
    {
        public event ReceivePacketDebugInfo<WSServerClient<T>> OnReceivePacket;
        public event SendPacketDebugInfo<WSServerClient<T>> OnSendPacket;
        /// <summary>
        /// Слушатель порта (сервер)
        /// </summary>
        private HttpListener listener;

        private bool state;

        /// <summary>
        /// Текущее состояния сервера (вкл/выкл)
        /// </summary>
        public bool State { get { return state; } }

        /// <summary>
        /// Настройки сервера
        /// </summary>
        private WSServerOptions<T> serverOptions;

        /// <summary>
        /// Инициализация сервера
        /// </summary>
        /// <param name="options">Настройки</param>
        public WSServerListener(WSServerOptions<T> options)
        {
            serverOptions = options;
        }

        /// <summary>
        /// Инициализация слушателя
        /// </summary>
        private void Initialize()
        {
            //Иницализация сокета, установка семейства адрессов, поточного сокета, протокола приема данных
            listener = new HttpListener();
            //Инициализация прослушивания на определенном адресе адаптера, порте

            foreach (var endPoint in serverOptions.EndPoints)
            {
                listener.Prefixes.Add(endPoint);
            }

            listener.Start();
        }

        /// <summary>
        /// Запустить сервер
        /// </summary>
        public void Run()
        {
            // Нельзя запустить если сервер уже запущен
            if (state)
                throw new Exception();
            //инициализация прослушивания
            Initialize();
            //Запуск ожидания приема клиентов
            listener.BeginGetContext(Accept, listener);
            // установка статуса сервер = вкл
            state = true;
        }

        /// <summary>
        /// Остановить сервер (важно, все подключенные клиенты не будут отключены)
        /// </summary>
        public void Stop()
        {
            // установка статуса сервер = выкл
            state = false;
            try
            {
                //Закрытие и уничножения слушателя
                listener.Close();
            }
            catch (Exception ex)
            {
                serverOptions.RunException(ex, null);
            }
            listener = null;
        }

        /// <summary>
        /// Синхронно принимаем входящие запросы на подключение
        /// </summary>
        /// <param name="result"></param>
        private async void Accept(IAsyncResult result)
        {
            //завершить цикл приема если сервер выключен
            if (!state)
                return;

            //клиент
            HttpListenerContext client = null;

            try
            {
                //получения ожидающего подключения
                client = listener.EndGetContext(result);
                //инициализация слушателя клиента клиента
                //#if DEBUG
                var c = new WSServerClient<T>(client, serverOptions);
                c.OnReceivePacket += OnReceivePacket;
                c.OnSendPacket += OnSendPacket;
                await c.RunPacketReceiver();
                //#else
                //                new ServerClient<T>(client, serverOptions).RunPacketReceiver();
                //#endif
            }
            catch (Exception ex)
            {
                serverOptions.RunException(ex, null);
            }
            //продолжаем принимать запросы
            listener.BeginGetContext(Accept, listener);
        }

        public int GetListenerPort() => 0;

        public void Start() => Run();

        public CoreOptions GetOptions() => serverOptions;

        public ServerOptions<T> GetServerOptions() => serverOptions;
    }
}
