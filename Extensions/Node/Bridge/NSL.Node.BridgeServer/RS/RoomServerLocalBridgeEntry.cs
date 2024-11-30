using NSL.BuilderExtensions.WebSocketsServer;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using NSL.BuilderExtensions.LocalBridge;
using NSL.LocalBridge;
using NSL.SocketCore.Utils;
using NSL.Node.BridgeServer.Shared;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerLocalBridgeEntry : RoomServerBaseEntry
    {
        private readonly NodeNetworkHandles<NetworkClient> handles;

        public RoomServerLocalBridgeEntry(NodeBridgeServerEntry entry, NodeNetworkHandles<NetworkClient> handles, string logPrefix = null) : base(entry, logPrefix)
        {
            this.handles = handles;
        }

        public LocalBridgeClient<NetworkClient, TAnotherClient> CreateLocalBridge<TAnotherClient>()
            where TAnotherClient : INetworkClient, new()
        {
            return handles.Fill(Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>())).CreateLocalBridge<NetworkClient, TAnotherClient>();
        }

        public override void Run()
        {
        }
    }
}
