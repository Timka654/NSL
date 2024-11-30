using NSL.BuilderExtensions.SocketCore;
using NSL.EndPointBuilder;
using NSL.Node.BridgeLobbyClient.Models;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using System;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient
{
    public abstract class BridgeLobbyBaseNetwork
    {
        public string ServerIdentity { get; }

        public string IdentityKey { get; }

        protected Action<BridgeLobbyNetworkHandlesConfigurationModel> OnHandleConfiguration { get; }

        protected INetworkClient network { get; private set; }

        protected RequestProcessor PacketWaitBuffer { get; private set; }

        public BridgeLobbyBaseNetwork(
            string serverIdentity,
            string identityKey,
            Action<BridgeLobbyNetworkHandlesConfigurationModel> onHandleConfiguration)
        {
            ServerIdentity = serverIdentity;
            IdentityKey = identityKey;
            OnHandleConfiguration = onHandleConfiguration;
            OnStateChanged += state =>
            {
                if (HasSuccessConnections)
                    return;

                if (state)
                    HasSuccessConnections = state;
            };
        }


        protected TBuilder FillOptions<TBuilder>(TBuilder builder, Action<TBuilder> onBuild)
            where TBuilder : IOptionableEndPointBuilder<BridgeLobbyNetworkClient>, IHandleIOBuilder<BridgeLobbyNetworkClient>
        {
            builder.AddResponsePacketHandle(
                NodeBridgeLobbyPacketEnum.Response,
                c => c.PacketWaitBuffer);

            builder.AddPacketHandle(
                NodeBridgeLobbyPacketEnum.FinishRoomMessage,
                Packets.FinishRoomMessagePacket.Handle);

            builder.AddPacketHandle(
                NodeBridgeLobbyPacketEnum.RoomMessage,
                Packets.RoomMessagePacket.Handle);

            builder.AddDisconnectHandle(DisconnectHandle);

            builder.AddConnectHandle(ConnectHandle);

            if (onBuild != null)
                onBuild(builder);

            return builder;
        }

        private async void DisconnectHandle(BridgeLobbyNetworkClient client)
        {
            signResult = false;

            OnStateChanged(State);

            await Task.Delay(4_000);

            await InitNetwork();
        }

        private async void ConnectHandle(BridgeLobbyNetworkClient client)
        {
            client.HandlesConfiguration = HandleConfiguration;

            client.PingPongEnabled = true;

            network = client;

            PacketWaitBuffer = client.PacketWaitBuffer;

            if (!await TrySign())
            {
                network.Network.Options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Invalid identity data");
                client.Network.Disconnect();
                return;
            }

            network.Network.Options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, "Success connected");

            OnStateChanged(State);
        }

        bool _signResult = false;

        bool signResult { get => _signResult; set { _signResult = value; OnStateChanged(State); } }


        bool initialized = false;

        protected abstract Task<bool> InitNetwork();

        public async void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            await InitNetwork();
        }

        private async Task<bool> TrySign()
        {
            var output = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.SignServerRequest);

            new LobbySignInRequestModel()
            {
                Identity = ServerIdentity,
                IdentityKey = IdentityKey,
                IsRecovery = HasSuccessConnections
            }.WriteFullTo(output);

            bool signResult = false;

            await PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                signResult = data.ReadBool();

                IdentityFailed = !signResult;

                OnStateChanged(State);

                return Task.FromResult(true);
            });

            return signResult;
        }

        public async Task<CreateRoomSessionResponseModel> CreateRoom(LobbyCreateRoomSessionRequestModel roomInfo)
        {
            var output = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.CreateRoomSessionRequest);

            roomInfo.WriteFullTo(output);

            CreateRoomSessionResponseModel response = default;

            await PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                response = CreateRoomSessionResponseModel.ReadFullFrom(data);

                return Task.FromResult(true);
            });

            return response;
        }

        public async Task AddPlayer(LobbyRoomPlayerAddRequestModel playerInfo)
        {
            var output = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.AddPlayerRequest);

            playerInfo.WriteFullTo(output);

            await PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                return Task.FromResult(true);
            });
        }

        public async Task RemovePlayer(LobbyRoomPlayerRemoveRequestModel playerInfo)
        {
            var output = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.RemovePlayerRequest);

            playerInfo.WriteFullTo(output);

            await PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                return Task.FromResult(true);
            });
        }

        public event Action<bool> OnStateChanged = (state) => { };

        public bool State => network?.GetState() == true && signResult;

        public bool IdentityFailed { get; private set; }

        private bool HasSuccessConnections { get; set; }

        internal BridgeLobbyNetworkHandlesConfigurationModel HandleConfiguration { get; set; } = new BridgeLobbyNetworkHandlesConfigurationModel();
    }
}
