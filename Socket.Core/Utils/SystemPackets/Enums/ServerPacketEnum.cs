using System;
using System.Collections.Generic;
using System.Text;

namespace SocketCore.Utils.SystemPackets.Enums
{

    public enum ServerPacketEnum : ushort
    {
        RecoverySessionResult = ushort.MaxValue - 2,
        SystemTime = ushort.MaxValue - 1,
        AliveConnection = ushort.MaxValue,
    }
}
