using NSL.SocketCore.Utils;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Net.Sockets;

namespace NSL.TCP.Server
{
    /// <summary>
    /// Класс обработки клиента
    /// </summary>
    public class TCPServerClient<T> : BaseTcpClient<T, TCPServerClient<T>>
        where T : IServerNetworkClient, new()
    {
        private T clientData;

        public override T Data => clientData;

        /// <summary>
        /// Общие настройки сервера
        /// </summary>
        public ServerOptions<T> ServerOptions => (ServerOptions<T>)base.options;

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="client">клиент</param>
        /// <param name="options">общие настройки сервера</param>
        public TCPServerClient(Socket client, ServerOptions<T> options) : base(options)
        {
            Initialize(client);
        }

        protected void Initialize(Socket client)
        {
            clientData = new T();

            //обзятельная переменная в NetworkClient, для отправки данных, можно использовать привидения типов (Client)NetworkClient но это никому не поможет
            Data.Network = this;
            Data.ServerOptions = options;

            //установка переменной содержащую поток клиента
            sclient = client;

            //установка массива для приема данных, размер указан в общих настройках сервера
            receiveBuffer = new byte[options.ReceiveBufferSize];
            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            inputCipher = options.InputCipher.CreateEntry();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            outputCipher = options.OutputCipher.CreateEntry();

            //Bug fix, в системе Windows это значение берется из реестра, мы не сможем принять больше за раз чем прописанно в нем, если данных будет больше, то цикл приема зависнет
            sclient.ReceiveBufferSize = options.ReceiveBufferSize;

            //Bug fix, отключение буфферизации пакетов для уменьшения трафика, если не отключить то получим фризы, в случае с игровым соединением эту опцию обычно нужно отключать
            sclient.NoDelay = true;

            disconnected = false;
            //Начало приема пакетов от клиента
            options.CallClientConnectEvent(Data);
        }

        public void RunPacketReceiver() => RunReceive();

        public override void ChangeUserData(INetworkClient setClient)
        {
            if (setClient is T valid)
            {
                clientData = valid;
                Data.Network = this;
                Data.ServerOptions = options;
            }
            else
                throw new ArgumentException("Invalid type", nameof(setClient));
        }

        protected override TCPServerClient<T> GetParent() => this;

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => ServerOptions.CallClientDisconnectEvent(Data);

        protected override void RunException(Exception ex) => ServerOptions.CallExceptionEvent(ex, Data);
    }
}
