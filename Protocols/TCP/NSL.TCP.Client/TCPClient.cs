using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Net;
using System.Net.Sockets;

namespace NSL.TCP.Client
{
    /// <summary>Не-дженериковый движок TCP-клиента.</summary>
    public class TCPClient : BaseTcpClient
    {
        public long Version { get; set; }

        public override BaseNetworkConnection Data => options.ClientData;

        public CoreOptions ConnectionOptions => base.options;

        public event ReceivePacketDebugInfo<TCPClient> OnReceivePacket;
        public event SendPacketDebugInfo<TCPClient> OnSendPacket;

        public TCPClient(CoreOptions options, bool legacyTransport = false) : base(options, legacyTransport)
        {
        }

        public void Reconnect(Socket client)
        {
            disconnected = false;

            ConnectionOptions.InitializeClient(ConnectionOptions.ConnectionFactory());
            ConnectionOptions.ClientData.Network = this;

            sclient = client;
            endPoint = (IPEndPoint)sclient?.RemoteEndPoint;

            inputCipher = ConnectionOptions.InputCipher.CreateEntry();
            outputCipher = ConnectionOptions.OutputCipher.CreateEntry();

            RunReceive();

            ConnectionOptions.RunClientConnect();
        }

        public override void SetClientData(BaseNetworkConnection from) => ConnectionOptions.InitializeClient(from);

        protected override void RunDisconnect() => ConnectionOptions.RunClientDisconnect();

        protected override void RunException(Exception ex) => ConnectionOptions.RunException(ex);

        protected override void OnReceive(ushort pid, int len)
        {
            ConnectionOptions.ClientData.LastReceiveMessage = DateTime.UtcNow;
            base.OnReceive(pid, len);
            OnReceivePacket?.Invoke(this, pid, len);
        }

        protected override void OnSend(OutputPacketBuffer rbuff, string stackTrace)
        {
            base.OnSend(rbuff, stackTrace);
            OnSendPacket?.Invoke(this, rbuff.PacketId, rbuff.PacketLength, stackTrace);
        }
    }
}

