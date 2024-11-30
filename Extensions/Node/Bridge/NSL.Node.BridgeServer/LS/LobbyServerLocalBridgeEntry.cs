using NSL.BuilderExtensions.LocalBridge;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.LocalBridge;
using NSL.Node.BridgeServer.Shared;
using NSL.SocketCore.Utils;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerLocalBridgeEntry : LobbyServerBaseEntry
    {
        private readonly NodeNetworkHandles<NetworkClient> handles;

        public LobbyServerLocalBridgeEntry(NodeBridgeServerEntry entry, NodeNetworkHandles<NetworkClient> handles, string logPrefix = null) : base(entry, logPrefix)
        {
            this.handles = handles;
        }

        public LocalBridgeClient<NetworkClient, TAnotherClient> CreateLocalBridge<TAnotherClient>()
            where TAnotherClient: INetworkClient, new()
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
