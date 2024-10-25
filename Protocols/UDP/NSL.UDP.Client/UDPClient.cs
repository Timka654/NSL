using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace NSL.UDP.Client
{
    public class UDPClient<TClient> : BaseUDPClient<TClient, UDPClient<TClient>>
        where TClient : IServerNetworkClient, new()
    {
        private TClient clientData;

        public override TClient Data => clientData;

        public UDPClient(IPEndPoint receivePoint, Socket listenerSocket, UDPClientOptions<TClient> options) : base(receivePoint, listenerSocket, options)
        {
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();

            PacketHandles = options.GetHandleMap();

            clientData = new TClient();

            //обзятельная переменная в NetworkClient, для отправки данных, можно использовать привидения типов (Client)NetworkClient но это никому не поможет
            Data.Network = this;
            //Data.ServerOptions = options;

            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            inputCipher = options.InputCipher.CreateEntry();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            outputCipher = options.OutputCipher.CreateEntry();

            disconnected = false;
            //Начало приема пакетов от клиента
            options.CallClientConnectEvent(Data);
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

        protected override UDPClient<TClient> GetParent() => this;

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => base.options.CallClientDisconnectEvent(Data);

        protected override void RunException(Exception ex) => base.options.CallExceptionEvent(ex, Data);

    }
}
