using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace NSL.UDP.Client
{
    public class UDPClient<TClient> : BaseUDPClient<TClient, UDPClient<TClient>>
        where TClient : BaseNetworkConnection, new()
    {
        private TClient clientData;

        public override TClient Data => clientData;

        private bool connectDeferred;

        public UDPClient(IPEndPoint receivePoint, Socket listenerSocket, UDPClientOptions<TClient> options, bool deferConnect = false) : base(receivePoint, listenerSocket, options)
        {
            connectDeferred = deferConnect;
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();

            PacketHandles = options.GetHandleMap();

            clientData = new TClient();

            //обзятельная переменная в NetworkClient, для отправки данных, можно использовать привидения типов (Client)NetworkClient но это никому не поможет
            Data.Network = this;
            //Data.Options = options;

            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            inputCipher = options.InputCipher.CreateEntry();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            outputCipher = options.OutputCipher.CreateEntry();

            disconnected = false;
            if (!connectDeferred)
                options.CallClientConnectEvent(Data);
        }

        public override void ChangeUserData(BaseNetworkConnection newClientData)
            => SetClientData(newClientData);

        public override void SetClientData(BaseNetworkConnection from)
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

        protected override UDPClient<TClient> GetParent() => this;

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            if (connectDeferred)
            {
                connectDeferred = false;
                options.CallClientConnectEvent(Data);
            }

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => base.options.CallClientDisconnectEvent(Data);

        protected override void RunException(Exception ex) => base.options.CallExceptionEvent(ex, Data);

    }
}
