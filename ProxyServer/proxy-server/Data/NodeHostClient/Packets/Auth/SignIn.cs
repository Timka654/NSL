using SocketServer;
using SocketServer.Utils;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using ps.Data.NodeHostClient.Network;
using ps.Data.NodeHostClient.Info.Enums.Packets;
using Utils.Logger;

namespace ps.Data.NodeHostClient.Packets.Auth
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
                Network.Client.NetworkClient.Disconnect();
                return;
            }

            LoggerStorage.Instance.main.AppendInfo($"Host success connected!");
        }

        public static void Send()
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ServerPacketsEnum.SignIn);

            packet.WriteString16(StaticData.NodePlayerManager.PublicIp);

            packet.WriteInt32(NodeServer.Network.Server.options.Port);

            packet.WriteInt32(StaticData.NodePlayerManager.MaxPlayerCount);

            packet.WriteInt16(StaticData.ConfigurationManager.GetValue<short>("network/node_host_client/access/id"));

            packet.WriteString16(StaticData.ConfigurationManager.GetValue<string>("network/node_host_client/access/token"));

            StaticData.NodeHostNetwork?.Send(packet);
        }
    }
}
