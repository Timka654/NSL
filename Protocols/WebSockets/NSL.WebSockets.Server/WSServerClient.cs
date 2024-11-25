using NSL.SocketCore.Utils;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.WebSockets.Server
{
    public class WSServerClient<TClient> : BaseWSClient<TClient, WSServerClient<TClient>>
        where TClient : IServerNetworkClient, new()
    {
        private TClient clientData;

        public override TClient Data => clientData;

        /// <summary>
        /// Общие настройки сервера
        /// </summary>
        public ServerOptions<TClient> ServerOptions => (ServerOptions<TClient>)base.options;


        protected WSServerClient(ServerOptions<TClient> options) : base(options)
        {

        }

        /// <summary>
        /// Инициализация прослушивания клиента
        /// </summary>
        /// <param name="client">клиент</param>
        /// <param name="options">общие настройки сервера</param>
        public WSServerClient(HttpListenerContext client, ServerOptions<TClient> options) : this(options)
        {
            if (!client.Request.IsWebSocketRequest)
                throw new Exception($"{client.Request.UserHostAddress} is not WebSocket request");

            base.context = client;

            base.remoteEndPoint = context?.Request.RemoteEndPoint;

            Initialize();

        }

        protected void Initialize()
        {
            clientData = new TClient();

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
        }

        public virtual async Task RunPacketReceiver()
        {
            try
            {
                sclient = (await context.AcceptWebSocketAsync(null))?.WebSocket;

                //Начало приема пакетов от клиента
                options.CallClientConnectEvent(Data);

                RunReceive();
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
        public override void ChangeUserData(INetworkClient newClientData)
            => SetClientData(newClientData);

        public override void SetClientData(INetworkClient from)
        {
            if (from == null)
            {
                clientData = null;
                return;
            }

            if (from is TClient td)
            {
                // current data for dispose and move data
                var oldData = clientData;

                clientData = td;
                clientData.Network = this;

                oldData.Network = null;

                from.ChangeOwner(oldData);

                return;
            }

            throw new Exception($"{nameof(from)} must have type {typeof(TClient)}");
        }

        protected override WSServerClient<TClient> GetParent() => this;

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => ServerOptions.CallClientDisconnectEvent(Data);

        protected override void RunException(Exception ex) => ServerOptions.CallExceptionEvent(ex, Data);
    }
}
