using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Net;
using System.Net.Sockets;

namespace NSL.TCP.Server
{
    public class TCPServerClient : BaseTcpClient
    {
        private BaseNetworkConnection clientData;

        public override BaseNetworkConnection Data => clientData;

        public CoreOptions ServerOptions => base.options;

        public event ReceivePacketDebugInfo<TCPServerClient> OnReceivePacket;
        public event SendPacketDebugInfo<TCPServerClient> OnSendPacket;

        public TCPServerClient(Socket client, CoreOptions options, bool legacyTransport = false) : base(options, legacyTransport)
        {
            Initialize(client);
        }

        protected void Initialize(Socket client)
        {
            clientData = options.ConnectionFactory();

            Data.Network = this;
            Data.Options = options;

            sclient = client;
            endPoint = (IPEndPoint)sclient?.RemoteEndPoint;

            receiveBuffer = new byte[options.ReceiveBufferSize];

            inputCipher = options.InputCipher.CreateEntry();
            outputCipher = options.OutputCipher.CreateEntry();

            sclient.ReceiveBufferSize = options.ReceiveBufferSize;
            sclient.NoDelay = true;

            disconnected = false;

            options.CallClientConnectEvent(Data);
        }

        public virtual void RunPacketReceiver() => RunReceive();

        public override void ChangeUserData(BaseNetworkConnection newClientData) => SetClientData(newClientData);

        public override void SetClientData(BaseNetworkConnection from)
        {
            if (from == null) { clientData = null; return; }

            var oldData = clientData;
            clientData = from;
            clientData.Network = this;
            oldData.Network = null;
            from.ChangeOwner(oldData);
        }

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;
            base.OnReceive(pid, len);
            OnReceivePacket?.Invoke(this, pid, len);
        }

        protected override void OnSend(OutputPacketBuffer rbuff, string stackTrace)
        {
            base.OnSend(rbuff, stackTrace);
            OnSendPacket?.Invoke(this, rbuff.PacketId, rbuff.PacketLength, stackTrace);
        }

        protected override void RunDisconnect() => ServerOptions.CallClientDisconnectEvent(Data);

        protected override void RunException(Exception ex) => ServerOptions.CallExceptionEvent(ex, Data);
    }
}

