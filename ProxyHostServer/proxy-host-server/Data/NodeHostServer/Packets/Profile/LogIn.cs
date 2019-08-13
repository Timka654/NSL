using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.NodeHostServer.Network;
using phs.Data.NodeHostServer.Info.Enums.Packets;
using phs.Data.NodeHostServer.Info;

namespace phs.Data.NodeHostServer.Packets.Profile
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

            StaticData.NodePlayerManager.ConnectPlayer(client);
        }

        public static void Send(NetworkClientData client, LoginResultEnum result)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ClientPacketsEnum.LogInResult);

            packet.WriteByte((byte)result);

            client.Network.Send(packet);
        }
    }
}
