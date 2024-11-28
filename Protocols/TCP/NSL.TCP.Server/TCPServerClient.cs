using NSL.SocketCore.Utils;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace NSL.TCP.Server
{
    public class TCPServerClient<TClient> : BaseTcpClient<TClient, TCPServerClient<TClient>>
        where TClient : IServerNetworkClient, new()
    {
        private TClient clientData;
        private readonly bool legacyThread;

        public override TClient Data => clientData;

        public ServerOptions<TClient> ServerOptions => (ServerOptions<TClient>)base.options;

        public TCPServerClient(Socket client, ServerOptions<TClient> options,bool legacyTransport = false) : base(options, legacyTransport)
        {
            Initialize(client);
        }

        protected void Initialize(Socket client)
        {
            clientData = new TClient();

            Data.Network = this;
            Data.ServerOptions = options;

            sclient = client;
            this.endPoint = (IPEndPoint)sclient?.RemoteEndPoint;

            receiveBuffer = new byte[options.ReceiveBufferSize];

            inputCipher = options.InputCipher.CreateEntry();

            outputCipher = options.OutputCipher.CreateEntry();

            //Bug fix, в системе Windows это значение берется из реестра, мы не сможем принять больше за раз чем прописанно в нем, если данных будет больше, то цикл приема зависнет
            sclient.ReceiveBufferSize = options.ReceiveBufferSize;

            //Bug fix, отключение буфферизации пакетов для уменьшения трафика, если не отключить то получим фризы, в случае с игровым соединением эту опцию обычно нужно отключать
            sclient.NoDelay = true;

            disconnected = false;

            options.CallClientConnectEvent(Data);
        }

        public virtual void RunPacketReceiver() => RunReceive();

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

        protected override TCPServerClient<TClient> GetParent() => this;

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => ServerOptions.CallClientDisconnectEvent(Data);

        protected override void RunException(Exception ex) => ServerOptions.CallExceptionEvent(ex, Data);
    }
}
