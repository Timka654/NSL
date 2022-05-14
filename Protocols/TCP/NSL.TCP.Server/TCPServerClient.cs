using SocketCore.Utils;
using SocketCore.Utils.Cipher;
using SocketServer;
using SocketServer.Utils;
using System;
using System.Net.Sockets;

namespace NSL.TCP.Server
{
    /// <summary>
    /// Класс обработки клиента
    /// </summary>
    public class TCPServerClient<T> : BaseTcpClient<T, TCPServerClient<T>>
        where T : IServerNetworkClient
    {
        private T Data { get; set; }

        /// <summary>
        /// Общие настройки сервера
        /// </summary>
        public ServerOptions<T> ServerOptions => (ServerOptions<T>)base.options;


        protected TCPServerClient()
        {

        }

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="client">клиент</param>
        /// <param name="options">общие настройки сервера</param>
        public TCPServerClient(Socket client, ServerOptions<T> options) : base()
        {
            Initialize(client, options);
        }

        protected void Initialize(Socket client, ServerOptions<T> options)
        {
            Data = Activator.CreateInstance<T>();

            //установка переменной с общими настройками сервера
            base.options = options;

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
            options.RunClientConnect(Data);
        }

        public void RunPacketReceiver() => RunReceive();

        public override void ChangeUserData(INetworkClient setClient)
        {
            if (setClient is T valid)
            {
                Data = valid;
                Data.Network = this;
                Data.ServerOptions = options;
            }
            else
                throw new ArgumentException("Invalid type", nameof(setClient));
        }

        public override object GetUserData() => Data;

        protected override TCPServerClient<T> GetParent() => this;

        protected override void AddWaitPacket(byte[] buffer, int offset, int length) => Data.AddWaitPacket(buffer, offset, length);

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => ServerOptions.RunClientDisconnect(Data);

        protected override void RunException(Exception ex) => ServerOptions.RunException(ex, Data);
    }
}
