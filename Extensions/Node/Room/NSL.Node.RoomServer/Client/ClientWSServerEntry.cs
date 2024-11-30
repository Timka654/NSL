using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;
using NSL.WebSockets.Server;

namespace NSL.Node.RoomServer.Client
{
    public class ClientWSServerEntry : ClientServerBaseEntry
    {
        private readonly NodeNetworkHandles<TransportNetworkClient> handles;
        private readonly string bindingPoint;

        public ClientWSServerEntry(NodeRoomServerEntry entry, NodeNetworkHandles<TransportNetworkClient> handles, string bindingPoint, string logPrefix = null) : base(entry, logPrefix)
        {
            this.handles = handles;
            this.bindingPoint = bindingPoint;
        }
        public ClientWSServerEntry(NodeRoomServerEntry entry, NodeNetworkHandles<TransportNetworkClient> handles, int bindingPort, string logPrefix = null) : this(entry, handles, $"http://*:{bindingPort}/", logPrefix) { }

        public override void Run()
        {
            if (Listener != null)
                return;

            Listener = handles.Fill(Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSServerOptions<TransportNetworkClient>>()
                .WithBindingPoint(bindingPoint)))
                .Build();

            Listener.Start();
        }
    }
}
