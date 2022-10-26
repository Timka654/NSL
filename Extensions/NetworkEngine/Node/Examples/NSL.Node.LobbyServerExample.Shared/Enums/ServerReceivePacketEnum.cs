using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.LobbyServerExample.Shared.Enums
{
    public enum ServerReceivePacketEnum : ushort
    {
        CreateRoom = 1,
        JoinRoom,
        LeaveRoom,
        SendChatMessage,
        StartRoom,
        RemoveRoom
    }
}
