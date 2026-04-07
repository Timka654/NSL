using NSL.Node.BridgeLobbyClient.Models;
using NSL.SocketClient;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Request;

namespace NSL.Node.BridgeLobbyClient
{
    public class BridgeLobbyNetworkClient : BaseNetworkConnection
    {
        internal BridgeLobbyNetworkHandlesConfigurationModel HandlesConfiguration { get; set; }

        public RequestProcessor PacketWaitBuffer { get; }

        public BridgeLobbyNetworkClient()
        {
            PacketWaitBuffer = new RequestProcessor(this);
        }

        public override void Dispose()
        {
            PacketWaitBuffer.Dispose();

            base.Dispose();
        }
    }
}
