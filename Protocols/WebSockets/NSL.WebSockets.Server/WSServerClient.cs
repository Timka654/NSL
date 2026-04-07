using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NSL.WebSockets.Server
{
    public class WSServerClient : BaseWSClient
    {
        private BaseNetworkConnection clientData;

        public override BaseNetworkConnection Data => clientData;

        public CoreOptions ServerOptions => base.options;

        public event ReceivePacketDebugInfo<WSServerClient> OnReceivePacket;
        public event SendPacketDebugInfo<WSServerClient> OnSendPacket;

        protected WSServerClient(CoreOptions options) : base(options) { }

        public WSServerClient(HttpListenerContext client, CoreOptions options) : this(options)
        {
            if (!client.Request.IsWebSocketRequest)
                throw new Exception($"{client.Request.UserHostAddress} is not WebSocket request");

            base.context = client;
            base.remoteEndPoint = context?.Request.RemoteEndPoint;

            Initialize();
        }

        protected void Initialize()
        {
            clientData = options.ConnectionFactory();

            Data.Network = this;
            Data.Options = options;

            receiveBuffer = new byte[options.ReceiveBufferSize];
            inputCipher = options.InputCipher.CreateEntry();
            outputCipher = options.OutputCipher.CreateEntry();

            disconnected = false;
        }

        public virtual async Task RunPacketReceiver()
        {
            try
            {
                sclient = (await context.AcceptWebSocketAsync(null))?.WebSocket;
                options.CallClientConnectEvent(Data);
                RunReceive();
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

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
