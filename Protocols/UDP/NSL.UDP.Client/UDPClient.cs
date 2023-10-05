using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace NSL.UDP.Client
{
    public class UDPClient<T> : BaseUDPClient<T, UDPClient<T>>
        where T : IServerNetworkClient, new()
    {
        private T data;

        public override T Data => data;

        public UDPClient(IPEndPoint receivePoint, Socket listenerSocket, UDPClientOptions<T> options) : base(receivePoint, listenerSocket, options)
        {
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();

            PacketHandles = options.GetHandleMap();

            data = new T();

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

        public override void ChangeUserData(INetworkClient old_client_data)
        {
            if (old_client_data == null)
            {
                data = null;
                return;
            }

            old_client_data.ChangeOwner(Data);
        }

        protected override UDPClient<T> GetParent() => this;

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => base.options.CallClientDisconnectEvent(Data);

        protected override void RunException(Exception ex) => base.options.CallExceptionEvent(ex, Data);

    }
}
