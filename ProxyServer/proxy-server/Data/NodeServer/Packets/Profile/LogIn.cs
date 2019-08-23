using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using ps.Data.NodeServer.Network;
using ps.Data.NodeServer.Info.Enums.Packets;
using ps.Data.NodeServer.Info;

namespace ps.Data.NodeServer.Packets.Profile
{
    /// <summary>
    /// Авторизация
    /// </summary>
    [LobbyPacket(Info.Enums.Packets.ServerPacketsEnum.LogIn)]
    public class LogIn : IPacket<NetworkClientData>
    {
        public void Receive(NetworkClientData client, InputPacketBuffer data)
        {
            client.RoomId = data.ReadInt32();

            client.UserId = data.ReadInt32();

            client.ServerId = data.ReadInt32();

            StaticData.NodePlayerManager.ConnectPlayer(client);
        }

        public static void Send(NodePlayerInfo player, LoginResultEnum result)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ClientPacketsEnum.LogInResult);

            packet.WriteByte((byte)result);

            if (result == LoginResultEnum.Ok)
            {
                packet.WriteString16(player.ProxyIp);
            }

            player.Client.Network.Send(packet);
        }
    }
}
