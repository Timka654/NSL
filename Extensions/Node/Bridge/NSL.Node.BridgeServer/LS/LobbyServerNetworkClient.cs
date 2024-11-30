using NSL.Node.BridgeServer.RS;
using NSL.SocketCore.Extensions.Buffer;
using NSL.WebSockets.Server.AspNetPoint;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerNetworkClient : AspNetWSNetworkServerClient
    {
        public string Identity { get; set; }

        public bool Signed { get; set; }

        public RequestProcessor RequestBuffer { get; set; }

        public ConcurrentDictionary<Guid, List<RoomSession>> Rooms { get; private set; } = new ConcurrentDictionary<Guid, List<RoomSession>>();

        public NodeBridgeServerEntry Entry { get; internal set; }

        public LobbyServerNetworkClient() : base()
        {
            RequestBuffer = new RequestProcessor(this);
        }

        public void LoadFrom(LobbyServerNetworkClient other)
        {
            Rooms = other.Rooms;

            foreach (var room in Rooms)
            {
                foreach (var session in room.Value.ToArray())
                {
                    session.OwnedLobbyNetwork = this;
                }
            }
        }

        public void AddPlayerId(Guid roomId, string playerId)
        {
            if (Rooms.TryGetValue(roomId, out var room))
            {
                foreach (var session in room.ToArray())
                {
                    session.AddPlayerId(playerId);
                }
            }
        }

        public void RemovePlayerId(Guid roomId, string playerId)
        {
            if (Rooms.TryGetValue(roomId, out var room))
            {
                foreach (var session in room.ToArray())
                {
                    session.RemovePlayerId(playerId);
                }
            }
        }

        public override void Dispose()
        {
            if (RequestBuffer != null)
                RequestBuffer.Dispose();

            base.Dispose();
        }

    }
}
