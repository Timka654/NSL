using NSL.Node.LobbyServerExample.Shared.Enums;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Server.AspNetPoint;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace NSL.Node.LobbyServerExample.Shared.Models
{
    public class LobbyRoomInfoModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public int MaxMembers { get; set; }

        public Guid OwnerId { get; set; }

        public LobbyRoomState State { get; set; }

        private ConcurrentDictionary<Guid, LobbyRoomMemberModel> members = new ConcurrentDictionary<Guid, LobbyRoomMemberModel>();

        public JoinResultEnum JoinMember(LobbyNetworkClientModel client)
        {
            var member = new LobbyRoomMemberModel()
            {
                Client = client
            };

            if (!members.TryAdd(member.Client.UID, member)) // already exists
                return JoinResultEnum.Ok;

            if (State != LobbyRoomState.Lobby)
                return JoinResultEnum.NotFound;

            if (members.Count == MaxMembers)
                return JoinResultEnum.MaxMemberCount;

            client.CurrentRoom = this;
            client.CurrentRoomId = Id;

            BroadcastJoinMember(member);

            return JoinResultEnum.Ok;
        }

        public void LeaveMember(LobbyNetworkClientModel client)
        {
            if (members.TryRemove(client.UID, out var member))
            {

                client.CurrentRoom = default;
                client.CurrentRoomId = default;
                BroadcastLeaveMember(member);
            }
        }

        public void StartRoom()
        {
            State = LobbyRoomState.Processing;

            BroadcastStartRoom();
        }

        public void RemoveRoom()
        {
            foreach (var member in members)
            {
                member.Value.Client.CurrentRoom = this;
                member.Value.Client.CurrentRoomId = Id;
            }

            BroadcastRemoveRoom();
        }

        internal void SendChatMessage(LobbyNetworkClientModel client, string v)
        {
            BroadcastChatMessage(client, v);
        }

        public bool ExistsMember(Guid uid)
        {
            return members.ContainsKey(uid);
        }

        public int MemberCount()
            => members.Count;

        #region Broadcast

        private void BroadcastStartRoom()
        {
            foreach (var item in members)
            {
                var packet = new OutputPacketBuffer();

                packet.PacketId = (ushort)ClientReceivePacketEnum.RoomStartedMessage;

                packet.WriteString16($"{Id}:{item.Value.Client.UID}");

                item.Value.Client.Network.Send(packet);
            }
        }
        private void BroadcastChatMessage(LobbyNetworkClientModel client, string text)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.ChatMessage;

            packet.WriteGuid(client.UID);
            packet.WriteString16(text);

            BroadcastMessage(packet);
        }

        private void BroadcastJoinMember(LobbyRoomMemberModel member)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.RoomMemberJoinMessage;

            packet.WriteGuid(member.Client.UID);

            BroadcastMessage(packet);
        }

        private void BroadcastLeaveMember(LobbyRoomMemberModel member)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.RoomMemberLeaveMessage;

            packet.WriteGuid(member.Client.UID);

            BroadcastMessage(packet);
        }

        private void BroadcastRemoveRoom()
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.RoomRemoveMessage;

            packet.WriteGuid(Id);

            BroadcastMessage(packet);
        }

        private async void BroadcastMessage(OutputPacketBuffer packet)
        {
            await Task.Run(() =>
            {
                foreach (var item in members)
                {
                    item.Value.Client.Network.Send(packet, false);
                }

                packet.Dispose();
            });
        }

        #endregion
    }

    public class LobbyRoomMemberModel
    {
        public LobbyNetworkClientModel Client { get; set; }
    }

    public enum LobbyRoomState
    {
        Lobby,
        Processing,
        Runned
    }
}
