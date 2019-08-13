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
    public class NetworkNodeServerData : INetworkClient
    {
        public ProxyServerInfo ServerInfo { get; set; }

        public NetworkNodeServerData()
        {
        }

        public override void ChangeOwner(INetworkClient client)
        {
            var c = (NetworkNodeServerData) client;
            c.ServerInfo = ServerInfo;

            base.ChangeOwner(client);
        }
    }
}
