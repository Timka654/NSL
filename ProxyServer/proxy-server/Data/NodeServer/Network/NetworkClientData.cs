using SocketServer.Utils;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using ps.Data.NodeServer.Info;
using ps.Data.NodeServer.Info.Enums;
using BinarySerializer;
using BinarySerializer.DefaultTypes;

namespace ps.Data.NodeServer.Network
{
    /// <summary>
    /// Вся информация по текущему клиенту
    /// </summary>
    public class NetworkClientData : INetworkClient
    {
        public NodePlayerInfo NodePlayer { get; set; }

        public int UserId { get; set; }

        public int RoomId { get; set; }

        public int ServerId { get; set; }

        public NetworkClientData()
        {
        }

        public override void ChangeOwner(INetworkClient client)
        {
            var c = (NetworkClientData) client;
            c.NodePlayer = NodePlayer;

            base.ChangeOwner(client);
        }
    }
}
