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
            StaticData.NodePlayerManager.AppendPlayer(client, new NodePlayerInfo()
            {
                UserId = data.ReadInt32(),
                RoomId = data.ReadInt32(),
                ServerId = data.ReadInt32(),
                Id = data.ReadGuid(),
            });
        }

        public static void Send(NodePlayerInfo player, bool result)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ClientPacketsEnum.PlayerConnectedResult);

            packet.WriteBool(result);

            packet.WriteGuid(player.Id);

            player.Server?.Client.Network.Send(packet);
        }
    }
}
