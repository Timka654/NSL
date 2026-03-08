using NSL.Logger.Interface;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Managers;
using NSL.Node.BridgeServer.RS;
using System;

namespace NSL.Node.BridgeServer
{
    public class NodeBridgeServerEntry
    {
        public LobbyServerBaseEntry LobbyServersListener { get; internal set; }

        public RoomServerBaseEntry RoomServersListener { get; internal set; }

        public LobbyManager LobbyManager { get; internal set; }

        public RoomManager RoomManager { get; internal set; }

        public ILogger Logger { get; internal set; }

        internal void Run()
        {
            if (LobbyManager == null)
                throw new InvalidOperationException($"Before run require init {nameof(LobbyManager)} for correct working");

            if (RoomManager == null)
                throw new InvalidOperationException($"Before run require init {nameof(RoomManager)} for correct working");

            LobbyServersListener?.Run();
            RoomServersListener?.Run();
        }
    }
}
