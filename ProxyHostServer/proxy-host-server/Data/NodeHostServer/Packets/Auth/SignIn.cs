using SocketServer;
using SocketServer.Utils;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.NodeHostServer.Network;
using phs.Data.NodeHostServer.Info.Enums.Packets;
using Utils.Logger;

namespace phs.Data.NodeHostServer.Packets.Auth
{
    /// <summary>
    /// Авторизация сервера
    /// </summary>
    [NodeHostPacket( ServerPacketsEnum.SignIn)]
    public class SignIn : IPacket<NetworkNodeServerData>
    {
        public void Receive(NetworkNodeServerData client, InputPacketBuffer data)
        {
            client.ServerInfo = new Info.ProxyServerInfo(client)
            {
                Ip = data.ReadString16(),
                Port = data.ReadInt32(),
                MaxPlayerCount = data.ReadInt32(),
                Id = data.ReadInt16()
            };
            bool firstLoading = data.ReadBool();
            string connectionToken = data.ReadString16();

            Send(client, StaticData.ProxyServerManager.ConnectServer(client, firstLoading, connectionToken));
        }

        public static void Send(NetworkNodeServerData client, bool result)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ClientPacketsEnum.SignInResult);

            packet.WriteBool(result);

            client.Network?.Send(packet);
        }
    }
}