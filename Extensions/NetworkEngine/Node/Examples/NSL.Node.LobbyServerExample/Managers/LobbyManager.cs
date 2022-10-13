using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.Node.LobbyServerExample.Models;
using NSL.Node.LobbyServerExample.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Server;
using NSL.WebSockets.Server.AspNetPoint;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NSL.Node.LobbyServerExample.Managers
{
    public class LobbyManager
    {
        private ConcurrentDictionary<Guid, LobbyNetworkClient> clientMap = new ConcurrentDictionary<Guid, LobbyNetworkClient>();


        private ConcurrentDictionary<Guid, LobbyRoomInfoModel> roomMap = new ConcurrentDictionary<Guid, LobbyRoomInfoModel>();

        private ConcurrentDictionary<Guid, LobbyRoomInfoModel> processingRoomMap = new ConcurrentDictionary<Guid, LobbyRoomInfoModel>();

        internal void BuildNetwork(AspNetWebSocketsServerEndPointBuilder<LobbyNetworkClient, WSServerOptions<LobbyNetworkClient>> builder)
        {
            builder.AddConnectHandle(OnClientConnectedHandle);

            builder.AddDisconnectHandle(OnClientDisconnectedHandle);

            builder.AddPacketHandle((ushort)ServerReceivePacketEnum.CreateRoom, CreateRoomRequestHandle);
            builder.AddPacketHandle((ushort)ServerReceivePacketEnum.JoinRoom, JoinRoomRequestHandle);
            builder.AddPacketHandle((ushort)ServerReceivePacketEnum.LeaveRoom, LeaveRoomRequestHandle);
            builder.AddPacketHandle((ushort)ServerReceivePacketEnum.SendChatMessage, SendChatMessageRequestHandle);
            builder.AddPacketHandle((ushort)ServerReceivePacketEnum.StartRoom, RunRoomRequestHandle);
            builder.AddPacketHandle((ushort)ServerReceivePacketEnum.RemoveRoom, RemoveRoomRequestHandle);
        }

        #region NetworkHandle

        private void OnClientConnectedHandle(LobbyNetworkClient client)
        {
            do
            {
                client.UID = Guid.NewGuid();
            } while (!clientMap.TryAdd(client.UID, client));

            client.UID = client.UID;

            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.NewUserIdentity;

            packet.WriteGuid(client.UID);

            client.Network.Send(packet);
        }

        private void OnClientDisconnectedHandle(LobbyNetworkClient client)
        {
            var uid = client?.UID;

            if (uid != default)
            {
                clientMap.Remove(uid.Value, out _);

                if (client.CurrentRoom != default && client.CurrentRoom.State == LobbyRoomState.Lobby)
                    client.CurrentRoom.LeaveMember(client);
            }
        }

        #endregion

        internal Task<bool> BridgeValidateSessionAsync(string sessionIdentity)
        {
            var splited = sessionIdentity.Split(':');

            var roomId = Guid.Parse(splited.First());

            var uid = Guid.Parse(splited.Last());

            if (processingRoomMap.TryGetValue(uid, out var room))
            {
                if (room.ExistsMember(uid))
                    return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        #region PacketHandle

        private void CreateRoomRequestHandle(LobbyNetworkClient client, InputPacketBuffer data)
        {
            LobbyRoomInfoModel room = data.ReadJson16<LobbyRoomInfoModel>();

            room.OwnerId = client.UID;

            Guid rid;

            do
            {
                rid = Guid.NewGuid();
            } while (!roomMap.TryAdd(rid, room));

            room.Id = rid;

            room.JoinMember(client);

            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.CreateRoomResult;

            packet.WriteGuid(rid);

            client.Network.Send(packet);

            BroadcastNewLobbyRoom(room);
        }

        private void RemoveRoomRequestHandle(LobbyNetworkClient client, InputPacketBuffer data)
        {
            if (client.CurrentRoom == null)
                return;

            var room = client.CurrentRoom;

            if (client.UID == room.OwnerId)
            {
                room.RemoveRoom();

                BroadcastRemoveLobbyRoom(room);
            }
        }

        private void RunRoomRequestHandle(LobbyNetworkClient client, InputPacketBuffer data)
        {
            if (client.CurrentRoom == null)
                return;

            var room = client.CurrentRoom;

            if (client.UID == room.OwnerId)
            {
                room.StartRoom();

                processingRoomMap.TryAdd(room.Id, room);

                roomMap.TryRemove(room.Id, out _);

                BroadcastRemoveLobbyRoom(room);
            }
        }
        private void SendChatMessageRequestHandle(LobbyNetworkClient client, InputPacketBuffer data)
        {
            if (client.CurrentRoom == null)
                return;

            client.CurrentRoom.SendChatMessage(client, data.ReadString16());
        }

        private void JoinRoomRequestHandle(LobbyNetworkClient client, InputPacketBuffer data)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.JoinRoomResult;

            var rid = data.ReadGuid();

            var password = data.ReadString16();

            if (roomMap.TryGetValue(rid, out var room))
            {
                if (room.Password != default && !room.Password.Equals(password))
                    packet.WriteByte((byte)JoinResultEnum.InvalidPassword);
                else
                {
                    packet.WriteByte((byte)room.JoinMember(client));
                    BroadcastChangeLobbyRoom(room);
                }
            }
            else
                packet.WriteByte((byte)JoinResultEnum.NotFound);

            client.Network.Send(packet);
        }

        private void LeaveRoomRequestHandle(LobbyNetworkClient client, InputPacketBuffer data)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.LeaveRoomResult;

            packet.WriteBool(true);

            if (client.CurrentRoom != null)
            {
                var room = client.CurrentRoom;

                room.LeaveMember(client);

                BroadcastChangeLobbyRoom(room);
            }
            client.Network.Send(packet);
        }

        #endregion

        #region Broadcast

        private void BroadcastNewLobbyRoom(LobbyRoomInfoModel room)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.NewRoomMessage;

            packet.WriteGuid(room.Id);
            packet.WriteGuid(room.OwnerId);
            packet.WriteString16(room.Name);
            packet.WriteInt32(room.MaxMembers);
            packet.WriteInt32(room.MemberCount());

            Broadcast(packet);
        }

        private void BroadcastRemoveLobbyRoom(LobbyRoomInfoModel room)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.NewRoomMessage;

            packet.WriteGuid(room.Id);

            Broadcast(packet);
        }

        private void BroadcastChangeLobbyRoom(LobbyRoomInfoModel room)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)ClientReceivePacketEnum.NewRoomMessage;

            packet.WriteGuid(room.Id);
            packet.WriteInt32(room.MaxMembers);
            packet.WriteInt32(room.MemberCount());

            Broadcast(packet);
        }

        private async void Broadcast(OutputPacketBuffer buffer)
        {
            await Task.Run(() =>
            {
                foreach (var item in clientMap)
                {
                    item.Value.Network.Send(buffer, false);
                }

                buffer.Dispose();
            });
        }

        #endregion
    }
}
