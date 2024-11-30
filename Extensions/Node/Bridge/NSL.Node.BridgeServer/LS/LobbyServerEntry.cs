using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Node.BridgeServer.Shared;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerEntry : LobbyServerBaseEntry
    {
        private readonly NodeNetworkHandles<NetworkClient> handles;
        private readonly string bindingPoint;

        public LobbyServerEntry(NodeBridgeServerEntry entry, NodeNetworkHandles<NetworkClient> handles, string bindingPoint, string logPrefix = null) : base(entry, logPrefix)
        {
            this.handles = handles;
            this.bindingPoint = bindingPoint;
        }

        public LobbyServerEntry(NodeBridgeServerEntry entry, NodeNetworkHandles<NetworkClient> handles, int bindingPort, string logPrefix = null) : this(entry, handles, $"http://*:{bindingPort}/", logPrefix) { }

        public override void Run()
        {
            Listener = handles.Fill(Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()))
                .WithBindingPoint(bindingPoint)
                .Build();

            Listener.Start();
        }
    }
}
