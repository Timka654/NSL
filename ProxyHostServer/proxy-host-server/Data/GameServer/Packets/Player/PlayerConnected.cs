using phs.Data.GameServer.Info.Enums.Packets;
using phs.Data.GameServer.Network;
using phs.Data.NodeHostServer.Info;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Logger;

namespace phs.Data.GameServer.Packets.Player
{
    /// <summary>
    /// Подключение игрока к room серверу
    /// </summary>
    [GamePacket( ServerPacketsEnum.PlayerConnectedResult)]
    public class PlayerConnected : IPacket<NetworkGameServerData>
    {
        public void Receive(NetworkGameServerData client, InputPacketBuffer data)
        {
            //var guid = new Guid(data.Read(data.ReadByte()));
            //bool result = data.ReadBool();

            //StaticData.NodePlayerManager.ConfirmPlayer(guid, result);
        }

        public static void Send(NetworkGameServerData server, NodePlayerInfo player)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ClientPacketsEnum.PlayerConnected);

            //packet.WriteInt32(player.Client?.UserId ?? 0);

            //packet.WriteInt32(player.Client?.RoomId ?? 0);

            var arr = player.Id.ToByteArray();

            packet.WriteByte((byte)arr.Length);

            packet.Write(arr, 0, arr.Length);

            server.Network.Send(packet);
        }
    }
}
