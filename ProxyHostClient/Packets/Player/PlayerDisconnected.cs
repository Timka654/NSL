using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;

namespace ProxyHostClient.Packets.Player
{
    [ProxyHostPacket(Enums.ClientPacketsEnum.PlayerDisconnected)]
    internal class PlayerDisconnected : IPacket<ProxyHostClientData>
    {
        public event ProxyHostClient.OnPlayerDisconnectedDelegate OnReceive;
        public override void Receive(ProxyHostClientData client, InputPacketBuffer data)
        {
            OnReceive?.Invoke(data.ReadGuid());
        }

        public static void Send(ProxyHostClientData client, Guid id)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(Enums.ServerPacketsEnum.PlayerDisconnected);

            packet.WriteGuid(id);

            client.Network.Send(packet);
        }
    }
}
