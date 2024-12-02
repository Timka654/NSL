using NSL.Logger.Interface;
using NSL.Logger;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketServer.Utils;
using NSL.EndPointBuilder;
using NSL.SocketCore.Utils.Buffer;
using System;
using NSL.SocketCore.Utils.Logger;

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
            where TBuilder : IOptionableEndPointBuilder<NetworkClient>, IHandleIOBuilder<NetworkClient>
        {
            builder.SetLogger(Logger);

            builder.AddConnectHandle(client =>
            {
                if (client != null)
                    client.Entry = Entry;
            });

            builder.AddDisconnectHandle(Entry.RoomManager.OnDisconnectedRoomServer);

            builder.AddAsyncPacketHandle(NodeBridgeRoomPacketEnum.SignServerRequest, SignServerReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignSessionRequest, SignSessionReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.FinishRoomMessage, FinishRoomReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.RoomMessage, RoomMessageReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignSessionPlayerRequest, SignSessionPlayerReceiveHandle);

            return builder;
        }
    }
}
