using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SocketServer.Utils.SystemPackets;

namespace SocketServer
{
    public class ServerListener<T> where T : INetworkClient
    {
#if DEBUG
        public event ReceivePacketDebugInfo<T> OnReceivePacket;
        public event SendPacketDebugInfo<T> OnSendPacket;
#endif
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
        public ServerListener(ServerOptions<T> options)
        {
            serverOptions = options;
            serverOptions.AddPacket((ushort)Utils.SystemPackets.Enums.ClientPacketEnum.AliveConnection,
                new AliveConnection<T>());
            serverOptions.AddPacket((ushort)Utils.SystemPackets.Enums.ClientPacketEnum.RecoverySession,
                new RecoverySession<T>());
            serverOptions.AddPacket((ushort)Utils.SystemPackets.Enums.ClientPacketEnum.Version,
                new Version<T>());
        }

        /// <summary>
        /// Инициализация слушателя
        /// </summary>
        private void Initialize()
        {
            //Иницализация сокета, установка семейства адрессов, поточного сокета, протокола приема данных
            listener = new Socket(serverOptions.AddressFamily, SocketType.Stream, serverOptions.ProtocolType);
            //Инициализация прослушивания на определенном адресе адаптера, порте
            listener.Bind(new IPEndPoint(IPAddress.Parse(serverOptions.IpAddress), serverOptions.Port));
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
                serverOptions.RunExtension(ex,null);
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
#if DEBUG
                var c = new Client<T>(client, serverOptions);
                c.OnReceivePacket += OnReceivePacket;
                c.OnSendPacket += OnSendPacket;
                c.RunPacketReceiver();
#else
                new Client<T>(client, serverOptions).RunPacketReceiver();
#endif
            }
            catch (Exception ex)
            {
                serverOptions.RunExtension(ex, null);
            }
            //продолжаем принимать запросы
            listener.BeginAccept(Accept, listener);
        }
    }
}
