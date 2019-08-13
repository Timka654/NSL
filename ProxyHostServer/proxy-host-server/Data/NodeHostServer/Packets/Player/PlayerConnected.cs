using phs.Data.NodeHostServer.Info;
using phs.Data.NodeHostServer.Info.Enums.Packets;
using phs.Data.NodeHostServer.Network;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Logger;

namespace phs.Data.NodeHostServer.Packets.Player
{
    /// <summary>
    /// Подключение игрока к room серверу
    /// </summary>
    [NodeHostPacket(ServerPacketsEnum.PlayerConnected)]
    public class PlayerConnected : IPacket<NetworkNodeServerData>
    {
        public void Receive(NetworkNodeServerData client, InputPacketBuffer data)
        {
            //var guid = new Guid(data.Read(data.ReadByte()));
            //bool result = data.ReadBool();

            //StaticData.NodePlayerManager.ConfirmPlayer(guid, result);
        }

        //public static void Send(NodePlayerInfo player)
        //{
        //    var packet = new OutputPacketBuffer();

        //    //packet.SetPacketId(ServerPacketsEnum.PlayerConnected);

        //    packet.WriteInt32(player.Client?.UserId ?? 0);

        //    packet.WriteInt32(player.Client?.RoomId ?? 0);

        //    var arr = player.Id.ToByteArray();

        //    packet.WriteByte((byte)arr.Length);

        //    packet.Write(arr, 0, arr.Length);

        //    //StaticData.NodeHostNetwork.Send(packet);
        //}
    }
}
