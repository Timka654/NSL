using ps.Data.NodeServer.Info.Enums.Packets;
using ps.Data.NodeServer.Network;
using ps.Data.NodeServer.Packets;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ps.Data.NodeServer.Packets.Transport
{
    [LobbyPacket(Info.Enums.Packets.ServerPacketsEnum.Transport)]
    public class TransportData : IPacket<NetworkClientData>
    {
        public void Receive(NetworkClientData client, InputPacketBuffer data)
        {
            StaticData.NodePlayerManager.Transport(client, data);
        }
    }
}
