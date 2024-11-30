using NSL.LocalBridge;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NSL.Node.RoomServer.Bridge
{
    public static class BuilderExtensions
    {
        public static NodeRoomServerEntryBuilder WithRoomBridgeLocalBridgeNetwork<TServerClient>(this NodeRoomServerEntryBuilder builder
            , LocalBridgeClient<TServerClient, BridgeRoomNetworkClient> serverClient
            , Dictionary<string, string> identityData
            , string publicEndPoint
            , NodeNetworkHandles<BridgeRoomNetworkClient> handles
            , Guid serverId = default
            , string logPrefix = null)
            where TServerClient : INetworkClient, new()
        {
            var local = new BridgeRoomLocalBridgeNetwork<TServerClient>(builder.Entry, identityData, publicEndPoint, handles, serverId, logPrefix);

            local.WithServerClient(serverClient);

            return builder.WithBridgeRoomClient(local);
        }

        public static NodeRoomServerEntryBuilder WithRoomBridgeNetwork(this NodeRoomServerEntryBuilder builder
            , string wsUrl
            , Dictionary<string, string> identityData
            , string publicEndPoint
            , NodeNetworkHandles<BridgeRoomNetworkClient> handles
            , Guid serverId = default
            , string logPrefix = null)
            => WithRoomBridgeNetwork(builder, new Uri(wsUrl), identityData, publicEndPoint, handles, serverId, logPrefix);

        public static NodeRoomServerEntryBuilder WithRoomBridgeNetwork(this NodeRoomServerEntryBuilder builder
            , Uri wsUrl
            , Dictionary<string, string> identityData
            , string publicEndPoint
            , NodeNetworkHandles<BridgeRoomNetworkClient> handles
            , Guid serverId = default
            , string logPrefix = null)
        {
            builder.WithBridgeRoomClient(new BridgeRoomNetwork(builder.Entry, wsUrl, identityData, publicEndPoint, serverId, logPrefix));

            return builder;
        }

        public static NodeRoomServerEntryBuilder WithBridgeDefaultHandles(this NodeRoomServerEntryBuilder builder)
        {
            builder
                .WithValidateSessionHandle(query => builder.Entry.BridgeNetworkClient.TrySignSession(query))
                .WithValidateSessionPlayerHandle(query => builder.Entry.BridgeNetworkClient.TrySignSessionPlayer(query))
                .WithRoomMessageHandle((roomInfo, data) => builder.Entry.BridgeNetworkClient.RoomMessage(roomInfo, data))
                .WithRoomFinishHandle((roomInfo, data) => builder.Entry.BridgeNetworkClient.FinishRoom(roomInfo, data));

            return builder;
        }
    }
}
