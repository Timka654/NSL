using SocketServer;
using SocketServer.Utils;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.GameServer.Network;
using phs.Data.GameServer.Info.Enums.Packets;
using Utils.Logger;

namespace phs.Data.GameServer.Packets.Auth
{
    /// <summary>
    /// Авторизация сервера
    /// </summary>
    [GamePacket(ServerPacketsEnum.SignIn)]
    public class SignIn : IPacket<NetworkGameServerData>
    {
        public void Receive(NetworkGameServerData client, InputPacketBuffer data)
        {
            client.ServerData = new Info.GameServerInfo(client)
            {
                Id = data.ReadInt16()
            };

            string connectionToken = data.ReadString16();

            Send(client, StaticData.GameServerManager.ConnectServer(client, connectionToken));
        }

        public static void Send(NetworkGameServerData client, bool result)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ClientPacketsEnum.SignInResult);

            packet.WriteBool(result);

            client.Network?.Send(packet);
        }
    }
}
