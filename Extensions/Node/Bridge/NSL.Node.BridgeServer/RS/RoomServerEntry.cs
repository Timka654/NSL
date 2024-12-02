using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Node.BridgeServer.Shared;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerEntry : RoomServerBaseEntry
    {
        private readonly NodeNetworkHandles<NetworkClient> handles;
        private readonly string bindingPoint;

        public RoomServerEntry(NodeBridgeServerEntry entry, NodeNetworkHandles<NetworkClient> handles, string bindingPoint, string logPrefix = null) : base(entry, logPrefix)
        {
            this.handles = handles;
            this.bindingPoint = bindingPoint;
        }
        public RoomServerEntry(NodeBridgeServerEntry entry, NodeNetworkHandles<NetworkClient> handles, int bindingPort, string logPrefix = null) : this(entry, handles, $"http://*:{bindingPort}/", logPrefix) { }

        public override void Run()
        {
            Listener = handles.Fill(Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithBindingPoint(bindingPoint)))
                .Build();

            Listener.Start();
        }
    }
}
