using NSL.SocketClient;
using NSL.SocketClient.Utils.SystemPackets;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace NSL.WebSockets.Client
{
    public class WSClient<T> : BaseWSClient<T, WSClient<T>>
        where T : BaseSocketNetworkClient, new()
    {
        public long Version { get; set; }

        public ClientOptions<T> ConnectionOptions => (ClientOptions<T>)base.options;

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="options">общие настройки сервера</param>
        public WSClient(ClientOptions<T> options) : base()
        {
            //установка переменной с общими настройками сервера
            this.options = options;
        }

        /// <summary>
        /// Запуск цикла приема пакетов
        /// </summary>
        /// <param name="client">клиент</param>
        public void Reconnect(WebSocket client, Uri endPoint)
        {
            if (!IPAddress.TryParse(endPoint.Host, out var ip))
            {
                var dns = Dns.GetHostEntry(endPoint.Host);

                if (dns.AddressList.Any())
                    ip = dns.AddressList.FirstOrDefault(x=>x.AddressFamily == AddressFamily.InterNetwork) ?? dns.AddressList.FirstOrDefault() ;
                else
                    ip = IPAddress.None;
            }

            remoteEndPoint = new IPEndPoint(ip, endPoint.Port);

            disconnected = false;
            string[] keys = ConnectionOptions.ClientData?.RecoverySessionKeyArray;
            IEnumerable<byte[]> waitBuffer = ConnectionOptions.ClientData?.GetWaitPackets();

            ConnectionOptions.InitializeClient(new T());

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
            this.inputCipher = ConnectionOptions.InputCipher.CreateEntry();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            this.outputCipher = ConnectionOptions.OutputCipher.CreateEntry();

            InitReceiver();

            _sendLocker.Set();

            RunReceiveAsync();

            VersionPacket.Send(ConnectionOptions.ClientData, Version);
            RecoverySessionPacket.Send(ConnectionOptions.ClientData);

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

        protected override WSClient<T> GetParent() => this;

        private IPEndPoint remoteEndPoint;

        public override IPEndPoint GetRemotePoint()
        {
            return remoteEndPoint;
        }
    }
}
