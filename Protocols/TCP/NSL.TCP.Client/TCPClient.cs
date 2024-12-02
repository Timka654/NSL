using NSL.SocketClient;
using NSL.SocketCore.Utils;
using System;
using System.Net;
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


        public TCPClient(ClientOptions<T> options, bool legacyTransport = false) : base(options, legacyTransport)
        {
        }


        public void Reconnect(Socket client)
        {
            disconnected = false;

            ConnectionOptions.InitializeClient(new T());

            ConnectionOptions.ClientData.Network = this;

            //установка переменной содержащую поток клиента
            this.sclient = client;
            this.endPoint = (IPEndPoint)sclient?.RemoteEndPoint;

            //this.receiveBuffer = new byte[ConnectionOptions.ReceiveBufferSize];

            this.inputCipher = ConnectionOptions.InputCipher.CreateEntry();

            this.outputCipher = ConnectionOptions.OutputCipher.CreateEntry();

            RunReceive();

            ConnectionOptions.RunClientConnect();
        }

        public override void ChangeUserData(INetworkClient newClientData) => SetClientData((T)newClientData);

        public override void SetClientData(INetworkClient from) => ConnectionOptions.InitializeClient((T)from);

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
