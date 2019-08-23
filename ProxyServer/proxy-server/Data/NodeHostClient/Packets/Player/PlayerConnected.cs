using ps.Data.NodeHostClient.Info.Enums.Packets;
using ps.Data.NodeHostClient.Network;
using ps.Data.NodeServer.Info;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Logger;

namespace ps.Data.NodeHostClient.Packets.Player
{
    /// <summary>
    /// Подключение игрока к room серверу
    /// </summary>
    [NodeHostPacket( ClientPacketsEnum.PlayerConnectedResult)]
    public class PlayerConnected : IPacket<NetworkNodeHostClientData>
    {
        public void Receive(NetworkNodeHostClientData client, InputPacketBuffer data)
        {
            bool result = data.ReadBool();
            var guid = new Guid(data.Read(data.ReadByte()));

            StaticData.NodePlayerManager.ConfirmPlayer(guid, result);
        }

        public static void Send(NodePlayerInfo player)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ServerPacketsEnum.PlayerConnected);

            packet.WriteInt32(player.Client?.UserId ?? 0);

            packet.WriteInt32(player.Client?.RoomId ?? 0);

            packet.WriteInt32(player.Client?.ServerId ?? 0);

            var arr = player.Id.ToByteArray();

            packet.WriteByte((byte)arr.Length);

            packet.Write(arr, 0, arr.Length);

            StaticData.NodeHostNetwork.Send(packet);
        }
    }
}
