using System;
using System.Collections.Generic;
using System.Text;

namespace phs.Data.NodeHostServer.Info
{
    public class NodePlayerInfo
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }

        public int RoomId { get; set; }

        public int ServerId { get; set; }

        public ProxyServerInfo Server { get; set; }
    }
}
