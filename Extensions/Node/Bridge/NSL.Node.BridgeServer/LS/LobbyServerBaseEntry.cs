using NSL.BuilderExtensions.SocketCore;
using NSL.EndPointBuilder;
using NSL.Logger;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketServer.Utils;

using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NSL.SocketCore.Utils.Logger;
using NSL.SocketCore;
using NSL.SocketCore.Network;

namespace NSL.Node.BridgeServer.LS
{
    public abstract partial class LobbyServerBaseEntry
    {
        protected INetworkListener Listener { get; set; }

        protected IBasicLogger Logger { get; }

        protected NodeBridgeServerEntry Entry { get; }

        public LobbyServerBaseEntry(NodeBridgeServerEntry entry, string logPrefix = null)
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix ?? "[LobbyServer]");
        }

        public abstract void Run();

        protected TBuilder Fill<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder, IHandleIOBuilder<NetworkClient>
        {
            builder.SetLogger(Logger);

            builder.AddConnectHandle(client =>
            {
                if (client != null)
                    ((NetworkClient)client).Entry = Entry;
            });

            builder.AddDisconnectHandle(_client =>
            {
                var client = (NetworkClient)_client;
                Entry.LobbyManager.OnDisconnectedLobbyServer(client);
            });

            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.SignServerRequest, (c,p)=> SignSessionRequestReceiveHandle((NetworkClient)c,p));
            builder.AddAsyncPacketHandle(NodeBridgeLobbyPacketEnum.CreateRoomSessionRequest, (c,p)=> CreateRoomSessionRequestReceiveHandle((NetworkClient)c,p));
            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.AddPlayerRequest, (c,p)=> AddPlayerRequestReceiveHandle((NetworkClient)c,p));
            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.RemovePlayerRequest, (c,p)=> RemovePlayerRequestReceiveHandle((NetworkClient)c,p));

            builder.AddResponsePacketHandle(
                NodeBridgeLobbyPacketEnum.Response,
                client => ((NetworkClient)client).RequestBuffer);

            return builder;
        }
    }
}
