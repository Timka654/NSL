using NSL.SocketClient;
using NSL.SocketClient.Utils.SystemPackets;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace NSL.TCP.Client
{
    /// <summary>
    /// Класс обработки клиента
    /// </summary>
    public class TCPClient<T> : BaseTcpClient<T, TCPClient<T>>
        where T : BaseSocketNetworkClient, new()
    {
        public long Version { get; set; }

        public override T Data => ConnectionOptions.ClientData;

        public ClientOptions<T> ConnectionOptions => base.options as ClientOptions<T>;

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="options">общие настройки сервера</param>
        public TCPClient(ClientOptions<T> options) : base(options) { }

        /// <summary>
        /// Запуск цикла приема пакетов
        /// </summary>
        /// <param name="client">клиент</param>
        public void Reconnect(Socket client)
        {
            disconnected = false;

            ConnectionOptions.InitializeClient(new T());

            ConnectionOptions.ClientData.Network = this;

            //установка переменной содержащую поток клиента
            this.sclient = client;

            //установка массива для приема данных, размер указан в общих настройках сервера
            this.receiveBuffer = new byte[ConnectionOptions.ReceiveBufferSize];
            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            this.inputCipher = ConnectionOptions.InputCipher.CreateEntry();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            this.outputCipher = ConnectionOptions.OutputCipher.CreateEntry();

            //Bug fix, в системе Windows это значение берется из реестра, мы не сможем принять больше за раз чем прописанно в нем, если данных будет больше, то цикл зависнет
            sclient.ReceiveBufferSize = ConnectionOptions.ReceiveBufferSize;

            //Bug fix, отключение буфферизации пакетов для уменьшения трафика, если не отключить то получим фризы, в случае с игровым соединением эту опцию обычно нужно отключать
            sclient.NoDelay = true;
            _sendLocker.Set();

            RunReceive();

            ConnectionOptions.RunClientConnect();
        }

        public override void ChangeUserData(INetworkClient data) => ConnectionOptions.InitializeClient((T)data);

        protected override void RunDisconnect() => ConnectionOptions.RunClientDisconnect();

        protected override void RunException(Exception ex) => ConnectionOptions.RunException(ex);

        protected override void OnReceive(ushort pid, int len)
        {
            ConnectionOptions.ClientData.LastReceiveMessage = DateTime.UtcNow;
            base.OnReceive(pid, len);
        }

        protected override TCPClient<T> GetParent() => this;
    }
}
