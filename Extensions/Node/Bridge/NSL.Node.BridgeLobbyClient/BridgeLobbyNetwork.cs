using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Node.BridgeLobbyClient.Models;
using NSL.WebSockets.Client;
using System;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient
{
    public class BridgeLobbyNetwork : BridgeLobbyBaseNetwork
    {
        private readonly Uri wsUrl;
        private readonly Action<WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild;
        private WSNetworkClient<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>> wsNetwork;

        public BridgeLobbyNetwork(Uri wsUrl, string serverIdentity, string identityKey, Action<BridgeLobbyNetworkHandlesConfigurationModel> onHandleConfiguration, Action<WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null) : base(serverIdentity, identityKey, onHandleConfiguration)
        {
            this.wsUrl = wsUrl;
            this.onBuild = onBuild;
        }

        protected override async Task<bool> InitNetwork()
        {
            if (wsNetwork == null)
            {
                OnHandleConfiguration(HandleConfiguration);

                wsNetwork = FillOptions(WebSocketsClientEndPointBuilder.Create()
                    .WithClientProcessor<BridgeLobbyNetworkClient>()
                    .WithOptions<WSClientOptions<BridgeLobbyNetworkClient>>()
                    .WithUrl(wsUrl), onBuild).Build();
            }

            if (!await wsNetwork.ConnectAsync(3000))
            {
                network.Network.Options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Cannot connect");

                return false;
            }

            return true;
        }
    }
}
