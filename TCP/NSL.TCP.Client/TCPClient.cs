using SocketClient;
using SocketClient.Utils;
using SocketClient.Utils.SystemPackets;
using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NSL.TCP.Client
{
    /// <summary>
    /// Класс обработки клиента
    /// </summary>
    public class TCPClient<T> : BaseTcpClient<T, TCPClient<T>>
        where T : BaseSocketNetworkClient
    {
        public long Version { get; set; }

        public ClientOptions<T> ConnectionOptions => (ClientOptions<T>)base.options;

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="options">общие настройки сервера</param>
        public TCPClient(ClientOptions<T> options) : base()
        {
            //установка переменной с общими настройками сервера
            this.options = options;
        }

        /// <summary>
        /// Запуск цикла приема пакетов
        /// </summary>
        /// <param name="client">клиент</param>
        public void Reconnect(Socket client)
        {
            disconnected = false;
            string[] keys = ConnectionOptions.ClientData?.RecoverySessionKeyArray;
            IEnumerable<byte[]> waitBuffer = ConnectionOptions.ClientData?.GetWaitPackets();

            ConnectionOptions.InitializeClient(Activator.CreateInstance<T>());

            ConnectionOptions.ClientData.SetRecoveryData(ConnectionOptions.ClientData.Session, keys);
            ConnectionOptions.ClientData.Network = this;

            if (waitBuffer != null)
                foreach (var item in waitBuffer)
                {
                    ConnectionOptions.ClientData.AddWaitPacket(item, 0, item.Length);
                }

            //установка переменной содержащую поток клиента
            this.sclient = client;

            //установка массива для приема данных, размер указан в общих настройках сервера
            this.receiveBuffer = new byte[ConnectionOptions.ReceiveBufferSize];
            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            this.inputCipher = (IPacketCipher)ConnectionOptions.inputCipher.Clone();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            this.outputCipher = (IPacketCipher)ConnectionOptions.outputCipher.Clone();

            //Bug fix, в системе Windows это значение берется из реестра, мы не сможем принять больше за раз чем прописанно в нем, если данных будет больше, то цикл зависнет
            sclient.ReceiveBufferSize = ConnectionOptions.ReceiveBufferSize;

            //Bug fix, отключение буфферизации пакетов для уменьшения трафика, если не отключить то получим фризы, в случае с игровым соединением эту опцию обычно нужно отключать
            sclient.NoDelay = true;
            _sendLocker.Set();

            RunReceive();

            VersionPacket.Send(ConnectionOptions.ClientData, Version);
            RecoverySessionPacket<T>.Send(ConnectionOptions.ClientData);

            ConnectionOptions.RunClientConnect();
        }

        public override object GetUserData() => ConnectionOptions.ClientData;

        public override void ChangeUserData(INetworkClient data) => ConnectionOptions.InitializeClient((T)data);

        protected override void RunDisconnect() => ConnectionOptions.RunClientDisconnect();

        protected override void RunException(Exception ex) => ConnectionOptions.RunException(ex);

        protected override void OnReceive(ushort pid, int len)
        {
            ConnectionOptions.ClientData.LastReceiveMessage = DateTime.UtcNow;
            base.OnReceive(pid, len);
        }

        protected override void AddWaitPacket(byte[] buffer, int offset, int length)
        {
            ConnectionOptions.ClientData?.AddWaitPacket(buffer, offset, length);
        }

        protected override TCPClient<T> GetParent() => this;
    }
}
