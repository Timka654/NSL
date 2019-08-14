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
                Id = new Guid(data.Read(data.ReadByte())),
            });
        }

        public static void Send(NodePlayerInfo player, bool result)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId( ClientPacketsEnum.PlayerConnectedResult);

            var arr = player.Id.ToByteArray();

            packet.WriteByte((byte)arr.Length);

            packet.Write(arr, 0, arr.Length);

            player.Server?.Client.Network.Send(packet);
        }
    }
}
