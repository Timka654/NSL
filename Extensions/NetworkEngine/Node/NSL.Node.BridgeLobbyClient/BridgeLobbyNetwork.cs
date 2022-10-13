using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient
{
    public delegate Task<bool> ValidateSessionDelegate(string sessionIdentity);

    public class BridgeLobbyNetwork
    {
        private const ushort SignServerPID = 1;
        private const ushort SignServerResultPID = SignServerPID;
        private const ushort ValidateSessionPID = 2;
        private const ushort ValidateSessionResultPID = ValidateSessionPID;

        private readonly Uri wsUrl;
        private readonly string serverIdentity;
        private readonly string identityKey;
        private WSNetworkClient<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>> network;

        public BridgeLobbyNetwork(
            Uri wsUrl,
            string serverIdentity,
            string identityKey,
            Action<WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null)
        {
            this.wsUrl = wsUrl;
            this.serverIdentity = serverIdentity;
            this.identityKey = identityKey;
            network = WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeLobbyNetworkClient>()
                .WithOptions<WSClientOptions<BridgeLobbyNetworkClient>>()
                .WithUrl(wsUrl)
                .WithCode(builder =>
                 {
                     builder.AddPacketHandle(SignServerResultPID, (c, d) =>
                     {
                         signResult = d.ReadBool();
                         signLocker.Set();
                     });

                     builder.AddPacketHandle(ValidateSessionPID, async (c, d) =>
                     {
                         var packet = d.CreateWaitBufferResponse();

                         packet.PacketId = ValidateSessionResultPID;

                         var sessionId = d.ReadString16();

                         bool result = default;

                         if (ValidateSession != null)
                             result = await ValidateSession(sessionId);

                         packet.WriteBool(true);

                         c.Network.Send(packet);
                     });

                     builder.AddDisconnectHandle(async client =>
                     {
                         signLocker.Reset();
                         signResult = false;

                         OnStateChanged(State);

                         do
                         {
                             await Task.Delay(4_000);
                         } while (await TryConnect());

                     });

                     builder.AddConnectHandle(async client =>
                     {
                         if (!await TrySign())
                         {
                             client.Network.Disconnect();
                             return;
                         }

                         OnStateChanged(State);
                     });

                     if (onBuild != null)
                         onBuild(builder);
                 })
                .Build();
        }

        AutoResetEvent signLocker = new AutoResetEvent(false);

        bool _signResult = false;

        bool signResult { get => _signResult; set { _signResult = value; OnStateChanged(State); } }


        bool initialized = false;

        public async void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            await TryConnect();
        }

        private async Task<bool> TryConnect(int timeout = 3000)
            => await network.ConnectAsync(timeout);

        private async Task<bool> TrySign()
        {
            return await Task.Run(() =>
            {
                IdentityFailed = false;

                var output = new OutputPacketBuffer();

                output.PacketId = SignServerPID;

                output.WriteString16(serverIdentity);

                output.WriteString16(identityKey);

                network.Send(output);

                signLocker.WaitOne();

                if (!signResult)
                    IdentityFailed = true;

                OnStateChanged(State);

                return signResult;
            });
        }

        public event Action<bool> OnStateChanged = (state) => { };

        public bool State => network?.GetState() == true && signResult;

        public bool IdentityFailed { get; private set; }

        public ValidateSessionDelegate ValidateSession { set; private get; }

    }
}
