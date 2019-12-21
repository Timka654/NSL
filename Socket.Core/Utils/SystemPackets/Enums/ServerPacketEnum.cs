using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer.Utils.SystemPackets.Enums
{

    public enum ServerPacketEnum : ushort
    {
        RecoverySessionResult = 65533,
        SystemTime = 65534,
        AliveConnection = 65535,
    }
}
