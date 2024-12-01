using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;

namespace NSL.Node.RoomServer.Client
{
    public static class BuilderExtensions
    {
        public static NodeRoomServerEntryBuilder WithTCPClientServerBinding(this NodeRoomServerEntryBuilder builder, int bindingPort, NodeNetworkHandles<TransportNetworkClient> handles,
            string logPrefix = null)
            => builder.WithClientServerListener(new ClientTcpServerEntry(builder.Entry, handles, bindingPort, logPrefix));
    }
}
