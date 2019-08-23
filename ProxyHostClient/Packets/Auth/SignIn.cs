using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Logger;

namespace ProxyHostClient.Packets.Auth
{
    [ProxyHostPacket(Enums.ClientPacketsEnum.SignInResult)]
    internal class SignIn : IPacket<ProxyHostClientData>
    {
        public void Receive(ProxyHostClientData client, InputPacketBuffer data)
        {
            bool result = data.ReadBool();

            if (!result)
            {
                LoggerStorage.Instance.main.AppendError("Invalid connection host data... Disconnect!");
                client.Network.Disconnect();
                return;
            }
            client.RunAliveChecker();
            LoggerStorage.Instance.main.AppendInfo($"Host success connected!");
        }

        public static void Send(ProxyHostClientData client, short id, string password)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Enums.ServerPacketsEnum.SignIn);

            packet.WriteInt16(id);

            packet.WriteString16(password);

            client.Network.Send(packet);
        }
    }
}
