using phs.Data.GameServer.Info;
using phs.Data.GameServer.Info.Enums.Packets;
using phs.Data.GameServer.Network;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.GameServer.Info.Enums;
using phs.Data.GameServer.Managers;
using Utils.Logger;
using phs.Data.NodeHostServer.Info;

namespace phs.Data.GameServer.Packets.Player
{
    [GamePacket(ServerPacketsEnum.PlayerDisconnected)]
    public class PlayerDisconnected : IPacket<NetworkGameServerData>
    {
        public void Receive(NetworkGameServerData client, InputPacketBuffer data)
        {
            var guid = data.ReadGuid();

            StaticData.NodePlayerManager.DisconnectPlayer(guid);
        }

        public static void Send(NetworkGameServerData server, Guid id)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(ClientPacketsEnum.PlayerDisconnected);

            packet.WriteGuid(id);

            server.Network.Send(packet);
        }
    }
}
