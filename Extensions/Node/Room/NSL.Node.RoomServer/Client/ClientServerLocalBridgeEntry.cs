using NSL.BuilderExtensions.LocalBridge;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.LocalBridge;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Server;

namespace NSL.Node.RoomServer.Client
{
    public class ClientServerLocalBridgeEntry : ClientServerBaseEntry
    {
        private readonly NodeNetworkHandles<TransportNetworkClient> handles;

        public ClientServerLocalBridgeEntry(NodeRoomServerEntry entry, NodeNetworkHandles<TransportNetworkClient> handles, string logPrefix = null) : base(entry, logPrefix)
        {
            this.handles = handles;
        }

        public LocalBridgeClient<TransportNetworkClient, TAnotherClient> CreateLocalBridge<TAnotherClient>()
            where TAnotherClient : INetworkClient, new()
        {
            return handles.Fill(Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSServerOptions<TransportNetworkClient>>())).CreateLocalBridge<TransportNetworkClient, TAnotherClient>();
        }

        public override void Run()
        {
        }
    }
}
