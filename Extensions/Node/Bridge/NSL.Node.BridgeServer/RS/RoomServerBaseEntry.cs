using NSL.Logger;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketServer.Utils;
using NSL.EndPointBuilder;
using NSL.SocketCore.Utils.Logger;
using NSL.SocketCore;
using NSL.SocketCore.Network;

namespace NSL.Node.BridgeServer.RS
{
    public abstract partial class RoomServerBaseEntry
    {
        protected INetworkListener Listener { get; set; }

        protected IBasicLogger Logger { get; }

        protected NodeBridgeServerEntry Entry { get; }

        public RoomServerBaseEntry(NodeBridgeServerEntry entry, string logPrefix = null)
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix ?? "[TransportServer]");
        }

        public abstract void Run();

        protected TBuilder Fill<TBuilder>(TBuilder builder)
            //where TBuilder : WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>
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
                Entry.RoomManager.OnDisconnectedRoomServer(client);
            });

            builder.AddAsyncPacketHandle(NodeBridgeRoomPacketEnum.SignServerRequest, (c,p)=> SignServerReceiveHandle((NetworkClient)c,p));
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignSessionRequest, (c,p)=> SignSessionReceiveHandle((NetworkClient)c,p));
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.FinishRoomMessage, (c,p)=> FinishRoomReceiveHandle((NetworkClient)c,p));
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.RoomMessage, (c,p)=> RoomMessageReceiveHandle((NetworkClient)c,p));
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignSessionPlayerRequest, (c,p)=> SignSessionPlayerReceiveHandle((NetworkClient)c,p));

            return builder;
        }
    }
}
