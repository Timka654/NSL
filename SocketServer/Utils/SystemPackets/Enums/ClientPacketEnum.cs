using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Utils.SystemPackets.Enums
{
    public enum ClientPacketEnum : ushort
    {
        ServerTime = 65534,
        AliveConnection = 65535
    }
}
