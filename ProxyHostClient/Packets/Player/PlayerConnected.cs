using ProxyHostClient.Packets.Player.PacketData;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyHostClient.Packets.Player
{
    [ProxyHostPacket(Enums.ClientPacketsEnum.PlayerConnected)]
    internal class PlayerConnected : IPacket<ProxyHostClientData>
    {
        public event ProxyHostClient.OnPlayerConnectedDelegate OnReceive;

        public void Receive(ProxyHostClientData client, InputPacketBuffer data)
        {
            PlayerConnectedPacketData result = new PlayerConnectedPacketData();

            result.UserId = data.ReadInt32();

            result.RoomId = data.ReadInt32();

            result.Id = data.ReadGuid();

            OnReceive?.Invoke(result);
        }

        public static void Send(ProxyHostClientData client, Guid id, bool result)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Enums.ServerPacketsEnum.PlayerConnectedResult);

            packet.WriteGuid(id);

            packet.WriteBool(result);

            client.Network.Send(packet);
        }
    }
}
