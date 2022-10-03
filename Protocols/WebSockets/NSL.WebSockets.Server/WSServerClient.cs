using NSL.SocketCore.Utils;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Net;

namespace NSL.WebSockets.Server
{
    public class WSServerClient<T> : BaseWSClient<T, WSServerClient<T>>
        where T : IServerNetworkClient, new()
    {
        private T Data { get; set; }

        /// <summary>
        /// Общие настройки сервера
        /// </summary>
        public ServerOptions<T> ServerOptions => (ServerOptions<T>)base.options;


        protected WSServerClient()
        {

        }

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="client">клиент</param>
        /// <param name="options">общие настройки сервера</param>
        public WSServerClient(HttpListenerContext client, ServerOptions<T> options) : base()
        {
            if (!client.Request.IsWebSocketRequest)
                throw new Exception($"{client.Request.UserHostAddress} is not WebSocket request");

            base.context = context;

            Initialize(options);
        }

        protected void Initialize(ServerOptions<T> options)
        {
            Data = new T();

            //установка переменной с общими настройками сервера
            base.options = options;

            //обзятельная переменная в NetworkClient, для отправки данных, можно использовать привидения типов (Client)NetworkClient но это никому не поможет
            Data.Network = this;
            Data.ServerOptions = options;

            //установка массива для приема данных, размер указан в общих настройках сервера
            receiveBuffer = new byte[options.ReceiveBufferSize];
            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            inputCipher = options.InputCipher.CreateEntry();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            outputCipher = options.OutputCipher.CreateEntry();

            disconnected = false;
            //Начало приема пакетов от клиента
            options.RunClientConnect(Data);
        }

        public virtual async void RunPacketReceiver()
        {
            try
            {
                sclient = (await context.AcceptWebSocketAsync(null))?.WebSocket;

                RunReceive();

            }
            catch (Exception)
            {
                Disconnect();
            }
        }
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

        protected override WSServerClient<T> GetParent() => this;

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
