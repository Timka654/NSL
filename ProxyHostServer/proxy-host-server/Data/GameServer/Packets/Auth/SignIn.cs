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
            var result = data.ReadBool();

            if (!result)
            {
                LoggerStorage.Instance.main.AppendError("Invalid connection host data... Disconnect!");
                client.Network?.Disconnect();
                return;
            }

            LoggerStorage.Instance.main.AppendInfo($"Host success connected!");
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
