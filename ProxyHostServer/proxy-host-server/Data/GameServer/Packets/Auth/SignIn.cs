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
    [NodeHostPacket(ClientPacketsEnum.SignInResult)]
    public class SignIn : IPacket<NetworkNodeHostClientData>
    {
        public void Receive(NetworkNodeHostClientData client, InputPacketBuffer data)
        {
            var result = data.ReadBool();

            if (!result)
            {
                LoggerStorage.Instance.main.AppendError("Invalid connection host data... Disconnect!");
                client.Network?.Disconnect();
                return;
            }

            LoggerStorage.Instance.main.AppendInfo($"Host success connected!");
        }

        public static void Send()
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ServerPacketsEnum.SignIn);

            packet.WriteString16(StaticData.NodePlayerManager.PublicIp);

            packet.WriteInt32(NodeHostServer.Network.Server.options.Port);

            packet.WriteInt32(StaticData.NodePlayerManager.MaxPlayerCount);

            packet.WriteString16(StaticData.ConfigurationManager.GetValue<string>("network/node_host_client/access/token"));

            StaticData.NodeHostNetwork?.Send(packet);
        }
    }
}
