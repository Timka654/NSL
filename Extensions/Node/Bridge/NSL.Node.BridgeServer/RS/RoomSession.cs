using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeServer.Shared.Message;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomSession : IDisposable
    {
        private AutoResetEvent playerIdsLocker = new(true);

        public event Action<RoomSession> OnDestroy = session => { };

        public Guid SessionId { get; set; }

        public Guid RoomIdentity { get; }

        public bool Active { get; set; }

        public LobbyServerNetworkClient OwnedLobbyNetwork { get; set; }

        public RoomServerNetworkClient OwnedRoomNetwork { get; set; }

        public NodeRoomStartupInfo StartupInfo { get; set; }

        public List<string>? PlayerIds { get; set; } = default;

        public DateTime CreateTime { get; } = DateTime.UtcNow;

        public void AddPlayerId(string playerId)
        {
            playerIdsLocker.WaitOne();

            if (PlayerIds == default)
                PlayerIds = new List<string>();

            if (!PlayerIds.Contains(playerId))
                PlayerIds.Add(playerId);

            playerIdsLocker.Set();
        }

        public void RemovePlayerId(string playerId)
        {
            if (PlayerIds == default)
                return;

            playerIdsLocker.WaitOne();

            if (!PlayerIds.Contains(playerId))
                PlayerIds.Remove(playerId);

            playerIdsLocker.Set();
        }

        internal void SendLobbyFinishRoom(byte[]? dataBuffer, bool manual)
        {
            var packet = OutputPacketBuffer.Create(NodeBridgeLobbyPacketEnum.FinishRoomMessage);

            new RoomFinishMessageModel
            {
                SessionId = RoomIdentity,
                Data = dataBuffer,
                Manual = manual
            }.WriteFullTo(packet);

            OwnedLobbyNetwork.Network?.Send(packet);
        }

        internal void SendLobbyRoomMessage(byte[] dataBuffer)
        {
            var packet = OutputPacketBuffer.Create(NodeBridgeLobbyPacketEnum.RoomMessage);

            new RoomMessageModel
            {
                SessionId = RoomIdentity,
                Data = dataBuffer
            }.WriteFullTo(packet);

            OwnedLobbyNetwork.Network?.Send(packet);
        }

        public RoomSession(LobbyServerNetworkClient ownedLobbyNetwork, RoomServerNetworkClient ownedRoomNetwork, LobbyCreateRoomSessionRequestModel request)
        {
            RoomIdentity = request.RoomId;
            OwnedLobbyNetwork = ownedLobbyNetwork;
            OwnedRoomNetwork = ownedRoomNetwork;
            StartupInfo = new Shared.NodeRoomStartupInfo(request.StartupOptions);
            PlayerIds = request.InitialPlayers;

            // Destroy session timer
            if (request.DelaySecondsForInactiveDestroy.HasValue)
                inactiveDestroyTimer = new Timer(state =>
                {
                    if (Active) return;

                    Dispose();
                    SendLobbyFinishRoom(null, false);
                }, default, TimeSpan.FromSeconds(request.DelaySecondsForInactiveDestroy.Value), Timeout.InfiniteTimeSpan);
        }

        Timer? inactiveDestroyTimer = null;

        public void Dispose()
        {
            OnDestroy(this);
        }

        internal bool ValidatePlayer(string playerId)
        {
            if (PlayerIds == default)
                return true;

            playerIdsLocker.WaitOne();

            bool result = PlayerIds.Contains(playerId);

            playerIdsLocker.Set();

            return result;
        }
    }
}
