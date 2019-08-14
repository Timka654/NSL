using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Logger;
using phs.Data.NodeHostServer.Info;
using phs.Data.NodeHostServer.Network;
using phs.Data.NodeHostServer.Info.Enums.Packets;

namespace phs.Data.NodeHostServer.Packets.Player
{
    [NodeHostPacket(ServerPacketsEnum.PlayerDisconnected)]
    public class PlayerDisconnected : IPacket<NetworkNodeServerData>
    {
        public void Receive(NetworkNodeServerData client, InputPacketBuffer data)
        {
            var id = new Guid(data.Read(data.ReadByte()));
            StaticData.NodePlayerManager.DisconnectPlayer(id);
        }
    }
}
