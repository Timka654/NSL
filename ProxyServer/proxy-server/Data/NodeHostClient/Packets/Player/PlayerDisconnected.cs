using ps.Data.NodeHostClient.Info;
using ps.Data.NodeHostClient.Info.Enums.Packets;
using ps.Data.NodeHostClient.Network;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using ps.Data.NodeHostClient.Info.Enums;
using ps.Data.NodeHostClient.Managers;
using Utils.Logger;
using ps.Data.NodeServer.Info;

namespace ps.Data.NodeHostClient.Packets.Player
{
    public class PlayerDisconnected
    {
        public static void Send(NodePlayerInfo player)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ServerPacketsEnum.PlayerDisconnected);

            var arr = player.Id.ToByteArray();

            packet.WriteByte((byte)arr.Length);
            packet.Write(arr, 0, arr.Length);

            StaticData.NodeHostNetwork.Send(packet);
        }
    }
}
