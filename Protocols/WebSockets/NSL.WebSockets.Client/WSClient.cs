using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace NSL.WebSockets.Client
{
    public class WSClient : BaseWSClient
    {
        public override BaseNetworkConnection Data => options.ClientData;

        public long Version { get; set; }

        public CoreOptions ConnectionOptions => base.options;

        public WSClient(CoreOptions options) : base(options)
        {
        }

        public void Reconnect(WebSocket client, Uri endPoint)
        {
            if (!IPAddress.TryParse(endPoint.Host, out var ip))
            {
                IPHostEntry dns = default;

                try { dns = Dns.GetHostEntry(endPoint.Host); } catch { }

                if (dns?.AddressList?.Any() == true)
                    ip = dns.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork) ?? dns.AddressList.FirstOrDefault();
                else
                    ip = IPAddress.None;
            }

            remoteEndPoint = new IPEndPoint(ip, endPoint.Port);

            disconnected = false;

            ConnectionOptions.InitializeClient(ConnectionOptions.ConnectionFactory());
            ConnectionOptions.ClientData.Network = this;

            this.sclient = client;

            this.receiveBuffer = new byte[ConnectionOptions.ReceiveBufferSize];
            this.inputCipher = ConnectionOptions.InputCipher.CreateEntry();
            this.outputCipher = ConnectionOptions.OutputCipher.CreateEntry();

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
        }

        public override IPEndPoint GetRemotePoint() => remoteEndPoint;
    }
}

