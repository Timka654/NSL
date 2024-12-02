using NSL.BuilderExtensions.LocalBridge;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.LocalBridge;
using NSL.Node.BridgeServer.Shared;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Bridge
{
    public class BridgeRoomLocalBridgeNetwork<TServerClient> : BridgeRoomBaseNetwork
        where TServerClient : INetworkClient, new()
    {
        private readonly NodeNetworkHandles<BridgeRoomNetworkClient> handles;
        private LocalBridgeClient<TServerClient, BridgeRoomNetworkClient> serverNetwork;
        LocalBridgeClient<BridgeRoomNetworkClient, TServerClient> localNetwork;

        public BridgeRoomLocalBridgeNetwork(NodeRoomServerEntry entry
            , Dictionary<string, string> identityData
            , string publicEndPoint
            , NodeNetworkHandles<BridgeRoomNetworkClient> handles
            , Guid serverId = default
            , string logPrefix = null) : base(entry, identityData, publicEndPoint, serverId, logPrefix)
        {
            this.handles = handles;
        }

        public BridgeRoomLocalBridgeNetwork<TServerClient> WithServerClient(LocalBridgeClient<TServerClient, BridgeRoomNetworkClient> serverClient)
        {
            serverNetwork = serverClient;

            return this;
        }

        protected override Task<bool> InitNetwork()
        {
            if (localNetwork == null)
            {
                var builder = handles.Fill(FillOptions(WebSocketsClientEndPointBuilder.Create()
                    .WithClientProcessor<BridgeRoomNetworkClient>()
                    .WithOptions()));

                localNetwork = builder.CreateLocalBridge<BridgeRoomNetworkClient, TServerClient>();
            }

            localNetwork.SetOtherClient(serverNetwork);

            return Task.FromResult(true);
        }
    }
}
