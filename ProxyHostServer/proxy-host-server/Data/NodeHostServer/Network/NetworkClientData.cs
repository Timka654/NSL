using SocketServer.Utils;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.NodeHostServer.Info;
using phs.Data.NodeHostServer.Info.Enums;
using BinarySerializer;
using BinarySerializer.DefaultTypes;

namespace phs.Data.NodeHostServer.Network
{
    /// <summary>
    /// Вся информация по текущему клиенту
    /// </summary>
    public class NetworkClientData : INetworkClient
    {
        public NodePlayerInfo NodePlayer { get; set; }

        public int UserId { get; set; }

        public int RoomId { get; set; }

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
