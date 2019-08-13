using phs.Data.GameServer.Info;
using phs.Data.GameServer.Info.Enums.Packets;
using phs.Data.GameServer.Network;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.GameServer.Info.Enums;
using phs.Data.GameServer.Managers;
using Utils.Logger;
using phs.Data.NodeHostServer.Info;

namespace phs.Data.GameServer.Packets.Player
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
