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
        public override T Data => ConnectionOptions.ClientData;

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
                IPHostEntry dns = default;

                try { dns = Dns.GetHostEntry(endPoint.Host); } catch { }

                if (dns?.AddressList?.Any() == true)
                    ip = dns.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork) ?? dns.AddressList.FirstOrDefault();
                else
                    ip = IPAddress.None;
            }

            remoteEndPoint = new IPEndPoint(ip, endPoint.Port);

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

            InitReceiver();

            _sendLocker.Set();

            RunReceiveAsync();

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

        protected override WSClient<T> GetParent() => this;

        private IPEndPoint remoteEndPoint;

        public override IPEndPoint GetRemotePoint()
        {
            return remoteEndPoint;
        }
    }
}
