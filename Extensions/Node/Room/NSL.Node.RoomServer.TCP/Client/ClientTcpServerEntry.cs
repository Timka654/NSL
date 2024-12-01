using NSL.BuilderExtensions.TCPServer;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;
using NSL.SocketServer;

namespace NSL.Node.RoomServer.Client
{
    public class ClientTcpServerEntry : ClientServerBaseEntry
    {
        private readonly NodeNetworkHandles<TransportNetworkClient> handles;
        private readonly int bindingPort;

        public ClientTcpServerEntry(NodeRoomServerEntry entry, NodeNetworkHandles<TransportNetworkClient> handles, int bindingPort, string logPrefix = null) : base(entry, logPrefix)
        {
            this.handles = handles;
            this.bindingPort = bindingPort;
        }

        public override void Run()
        {
            if (Listener != null)
                return;

            Listener = handles.Fill(Fill(TCPServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<ServerOptions<TransportNetworkClient>>()
                .WithBindingPoint(bindingPort)))
                .Build();

            Listener.Start();
        }
    }
}
