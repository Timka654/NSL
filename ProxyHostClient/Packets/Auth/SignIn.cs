using SCLogger;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyHostClient.Packets.Auth
{
    [ProxyHostPacket(Enums.ClientPacketsEnum.SignInResult)]
    internal class SignIn : IPacket<ProxyHostClientData>
    {
        public event EventHandler<bool> OnReceive;
        public override void Receive(ProxyHostClientData client, InputPacketBuffer data)
        {
            bool result = data.ReadBool();

            if (result)
            {
                client.RunAliveChecker();
                LoggerStorage.Instance.main.AppendInfo($"Host success connected!");
            }
            else
            {
                LoggerStorage.Instance.main.AppendError("Invalid connection host data... Disconnect!");
                client.Network.Disconnect();
            }
            OnReceive?.Invoke(this, result);

        }

        public static void Send(ProxyHostClientData client, int id, string password)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Enums.ServerPacketsEnum.GetProxyServer);

            packet.WriteInt32(id);

            packet.WriteString16(password);

            client.Network.Send(packet);
        }
    }
}
