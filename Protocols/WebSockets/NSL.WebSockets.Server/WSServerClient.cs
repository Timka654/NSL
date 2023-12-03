using NSL.SocketCore.Utils;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NSL.WebSockets.Server
{
    public class WSServerClient<T> : BaseWSClient<T, WSServerClient<T>>
        where T : IServerNetworkClient, new()
    {
        private T clientData;

        public override T Data => clientData;

        /// <summary>
        /// Общие настройки сервера
        /// </summary>
        public ServerOptions<T> ServerOptions => (ServerOptions<T>)base.options;


        protected WSServerClient(ServerOptions<T> options) : base(options)
        {

        }

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="client">клиент</param>
        /// <param name="options">общие настройки сервера</param>
        public WSServerClient(HttpListenerContext client, ServerOptions<T> options) : this(options)
        {
            if (!client.Request.IsWebSocketRequest)
                throw new Exception($"{client.Request.UserHostAddress} is not WebSocket request");

            base.context = client;

            base.remoteEndPoint = context?.Request.RemoteEndPoint;

            Initialize();

        }

        protected void Initialize()
        {
            clientData = new T();

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

            InitReceiver();
        }

        public virtual async Task RunPacketReceiver()
        {
            try
            {
                sclient = (await context.AcceptWebSocketAsync(null))?.WebSocket;

                //Начало приема пакетов от клиента
                options.CallClientConnectEvent(Data);

                RunReceiveAsync();
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
                clientData = valid;
                Data.Network = this;
                Data.ServerOptions = options;
            }
            else
                throw new ArgumentException("Invalid type", nameof(setClient));
        }

        protected override WSServerClient<T> GetParent() => this;

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => ServerOptions.CallClientDisconnectEvent(Data);

        protected override void RunException(Exception ex) => ServerOptions.CallExceptionEvent(ex, Data);
    }
}
