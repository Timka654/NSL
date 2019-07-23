using System;
using System.Collections.Generic;
using System.Text;

namespace SCL.SocketClient.Utils.SystemPackets.Enums
{
    public enum ClientPacketEnum : ushort
    {
        Version = 65532,
        RecoverySession = 65533,
        ServerTime = 65534,
        AliveConnection = 65535
    }
}
