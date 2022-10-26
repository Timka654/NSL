using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeClientPacketEnum : ushort
    {
        SignSessionPID = 1,
        SignSessionResultPID = SignSessionPID
    }
}
