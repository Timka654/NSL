using NSL.SocketCore;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace NSL.TCP.Server
{
    public class TCPServerListener<T> : INetworkListener<T>
        where T : IServerNetworkClient
    {
        public event ReceivePacketDebugInfo<TCPServerClient<T>> OnReceivePacket;
        public event SendPacketDebugInfo<TCPServerClient<T>> OnSendPacket;
        /// <summary>
        /// Слушатель порта (сервер)
        /// </summary>
        private Socket listener;

        private bool state;

        /// <summary>
        /// Текущее состояния сервера (вкл/выкл)
        /// </summary>
        public bool State { get { return state; } }

        /// <summary>
        /// Настройки сервера
        /// </summary>
        private ServerOptions<T> serverOptions;

        /// <summary>
        /// Инициализация сервера
        /// </summary>
        /// <param name="options">Настройки</param>
        public TCPServerListener(ServerOptions<T> options)
        {
            serverOptions = options;
        }

        /// <summary>
        /// Инициализация слушателя
        /// </summary>
        private void Initialize()
        {
            if (!IPAddress.TryParse(serverOptions.IpAddress, out var ip))
                throw new ArgumentException($"invalid connection ip {serverOptions.IpAddress}", nameof(serverOptions.IpAddress));

            if (serverOptions.AddressFamily == AddressFamily.Unspecified)
                serverOptions.AddressFamily = ip.AddressFamily;

            if (serverOptions.ProtocolType == ProtocolType.Unspecified)
                serverOptions.ProtocolType = ProtocolType.Tcp;

            //Иницализация сокета, установка семейства адрессов, поточного сокета, протокола приема данных
            listener = new Socket(serverOptions.AddressFamily, SocketType.Stream, serverOptions.ProtocolType);
            //Инициализация прослушивания на определенном адресе адаптера, порте
            listener.Bind(new IPEndPoint(IPAddress.Parse(serverOptions.IpAddress), serverOptions.Port));

            serverOptions.Port = listener.LocalEndPoint is IPEndPoint ipep ? ipep.Port : serverOptions.Port;

            // Запуск прослушивания
            listener.Listen(serverOptions.Backlog);
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
            listener.BeginAccept(Accept, listener);
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
                listener.Dispose();
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
        private void Accept(IAsyncResult result)
        {
            //завершить цикл приема если сервер выключен
            if (!state)
                return;

            //клиент
            Socket client = null;

            try
            {
                //получения ожидающего подключения
                client = listener.EndAccept(result);
                //инициализация слушателя клиента клиента
                //#if DEBUG
                var c = new TCPServerClient<T>(client, serverOptions);
                c.OnReceivePacket += OnReceivePacket;
                c.OnSendPacket += OnSendPacket;
                c.RunPacketReceiver();
                //#else
                //                new ServerClient<T>(client, serverOptions).RunPacketReceiver();
                //#endif
            }
            catch (Exception ex)
            {
                serverOptions.RunException(ex, null);
            }
            //продолжаем принимать запросы
            listener.BeginAccept(Accept, listener);
        }

        public int GetListenerPort() => serverOptions.Port;

        public void Start() => Run();

        public CoreOptions GetOptions() => serverOptions;

        public ServerOptions<T> GetServerOptions() => serverOptions;
    }
}
