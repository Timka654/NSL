using NSL.LocalBridge;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NSL.SocketCore.Utils;
using NSL.Node.BridgeServer.Shared;

namespace NSL.Node.BridgeServer.RS
{
    public static class BuilderExtensions
    {
        public static NodeBridgeServerEntryBuilder WithRoomServerLocalBridgeBinding<TAnotherClient>(
            this NodeBridgeServerEntryBuilder builder
            , NodeNetworkHandles<NetworkClient> handles
            , out LocalBridgeClient<NetworkClient, TAnotherClient> bridge
            , string logPrefix = null)
            where TAnotherClient : INetworkClient, new()
        {
            var local = new RoomServerLocalBridgeEntry(builder.Entry, handles, logPrefix);

            bridge = local.CreateLocalBridge<TAnotherClient>();

            return builder.WithRoomServerListener(local);
        }

        public static NodeBridgeServerEntryBuilder WithRoomServerBinding(this NodeBridgeServerEntryBuilder builder, NodeNetworkHandles<NetworkClient> handles, int bindingPort,
            string logPrefix = null)
            => builder.WithRoomServerListener(new RoomServerEntry(builder.Entry, handles, bindingPort, logPrefix));

        public static NodeBridgeServerEntryBuilder WithRoomServerBinding(this NodeBridgeServerEntryBuilder builder, NodeNetworkHandles<NetworkClient> handles, string bindingPoint,
            string logPrefix = null)
            => builder.WithRoomServerListener(new RoomServerEntry(builder.Entry,handles, bindingPoint, logPrefix));
    }
}
