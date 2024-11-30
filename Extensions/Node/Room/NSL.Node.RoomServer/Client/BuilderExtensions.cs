using NSL.LocalBridge;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils;

namespace NSL.Node.RoomServer.Client
{
    public static class BuilderExtensions
    {
        public static NodeRoomServerEntryBuilder WithClientServerLocalBridgeBinding<TAnotherClient>(
            this NodeRoomServerEntryBuilder builder,
            NodeNetworkHandles<TransportNetworkClient> handles,
            out LocalBridgeClient<TransportNetworkClient, TAnotherClient> bridge,
            string logPrefix = null)
            where TAnotherClient : INetworkClient, new()
        {
            var local = new ClientServerLocalBridgeEntry(builder.Entry, handles, logPrefix);

            bridge = local.CreateLocalBridge<TAnotherClient>();

            return builder.WithClientServerListener(local);
        }

        public static NodeRoomServerEntryBuilder WithWSClientServerBinding(this NodeRoomServerEntryBuilder builder, int bindingPort, NodeNetworkHandles<TransportNetworkClient> handles,
            string logPrefix = null)
            => builder.WithClientServerListener(new ClientWSServerEntry(builder.Entry, handles, bindingPort, logPrefix));

        public static NodeRoomServerEntryBuilder WithWSClientServerBinding(this NodeRoomServerEntryBuilder builder, string bindingPoint, NodeNetworkHandles<TransportNetworkClient> handles,
            string logPrefix = null)
            => builder.WithClientServerListener(new ClientWSServerEntry(builder.Entry, handles, bindingPoint, logPrefix));

        public static NodeRoomServerEntryBuilder WithTCPClientServerBinding(this NodeRoomServerEntryBuilder builder, int bindingPort, NodeNetworkHandles<TransportNetworkClient> handles,
            string logPrefix = null)
            => builder.WithClientServerListener(new ClientTcpServerEntry(builder.Entry, handles, bindingPort, logPrefix));
    }
}
