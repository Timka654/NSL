using NSL.LocalBridge;
using NSL.Node.BridgeServer.Shared;
using NSL.SocketCore.Utils;
using System.Reflection.Metadata;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;

namespace NSL.Node.BridgeServer.LS
{
    public static class BuilderExtensions
    {
        public static NodeBridgeServerEntryBuilder WithLobbyServerLocalBridgeBinding<TAnotherClient>(
            this NodeBridgeServerEntryBuilder builder,
            out LocalBridgeClient<NetworkClient, TAnotherClient> bridge,
            NodeNetworkHandles<NetworkClient> handles,
            string logPrefix = null)
            where TAnotherClient : INetworkClient, new()
        {
            var local = new LobbyServerLocalBridgeEntry(builder.Entry, handles, logPrefix);

            bridge = local.CreateLocalBridge<TAnotherClient>();

            return builder.WithLobbyServerListener(local);
        }

        public static NodeBridgeServerEntryBuilder WithLobbyServerBinding(this NodeBridgeServerEntryBuilder builder
            , NodeNetworkHandles<NetworkClient> handles
            , int bindingPort
            , string logPrefix = null)
            => builder.WithLobbyServerListener(new LobbyServerEntry(builder.Entry, handles, bindingPort, logPrefix));


        public static NodeBridgeServerEntryBuilder WithLobbyServerBinding(
            this NodeBridgeServerEntryBuilder builder,
            NodeNetworkHandles<NetworkClient> handles,
            string bindingPoint,
            string logPrefix = null)
            => builder.WithLobbyServerListener(new LobbyServerEntry(builder.Entry, handles, bindingPoint, logPrefix));
    }
}
