using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Managers;
using NSL.Node.BridgeServer.RS;
using System;

namespace NSL.Node.BridgeServer
{
    public class NodeBridgeServerEntryBuilder
    {
        public static NodeBridgeServerEntryBuilder Create() => new NodeBridgeServerEntryBuilder();

        public NodeBridgeServerEntry Entry { get; } = new NodeBridgeServerEntry();

        private bool processed;

        public NodeBridgeServerEntry Run()
        {
            Entry.Run();

            processed = true;

            return Entry;
        }

        public NodeBridgeServerEntryBuilder WithDefaultManagers(string lobbyIdentityKey)
            => WithLobbyManager(new LobbyManager(lobbyIdentityKey))
              .WithRoomManager(new RoomManager());

        public NodeBridgeServerEntryBuilder WithLobbyServerListener(LobbyServerBaseEntry entry)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            Entry.LobbyServersListener = entry;

            return this;
        }

        public NodeBridgeServerEntryBuilder WithRoomServerListener(RoomServerBaseEntry entry)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            Entry.RoomServersListener = entry;

            return this;
        }

        public NodeBridgeServerEntryBuilder WithLobbyManager(LobbyManager manager)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            Entry.LobbyManager = manager;

            return this;
        }


        public NodeBridgeServerEntryBuilder WithRoomManager(RoomManager manager)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            Entry.RoomManager = manager;

            return this;
        }


        public NodeBridgeServerEntryBuilder WithLogger(ILogger logger)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            Entry.Logger = logger;

            return this;
        }

        public NodeBridgeServerEntryBuilder WithConsoleLogger()
            => WithLogger(new ConsoleLogger());

    }
}
