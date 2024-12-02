using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Logger;
using NSL.WebSockets.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Bridge
{
    public class BridgeRoomNetwork : BridgeRoomBaseNetwork
    {
        private readonly Uri wsUrl;
        private WSNetworkClient<BridgeRoomNetworkClient, WSClientOptions<BridgeRoomNetworkClient>> wsNetwork;

        public BridgeRoomNetwork(NodeRoomServerEntry entry, Uri wsUrl, Dictionary<string, string> identityData, string publicEndPoint, Guid serverId = default, string logPrefix = null) : base(entry, identityData, publicEndPoint, serverId, logPrefix)
        {
            this.wsUrl = wsUrl;
        }

        protected override async Task<bool> InitNetwork()
        {
            Logger?.AppendInfo($"Try connect to Bridge({wsUrl})");
            if (wsNetwork == null)
            {
                wsNetwork = FillOptions(WebSocketsClientEndPointBuilder.Create()
                    .WithClientProcessor<BridgeRoomNetworkClient>()
                    .WithOptions()
                    .WithUrl(wsUrl))
                    .Build();
            }

            if (!await wsNetwork.ConnectAsync(3000))
            {
                Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Cannot connect");

                Reconnect();

                return false;
            }

            return true;
        }
    }
}
