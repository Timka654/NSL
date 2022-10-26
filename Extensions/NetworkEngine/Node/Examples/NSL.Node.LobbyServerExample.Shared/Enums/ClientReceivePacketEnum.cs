using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.LobbyServerExample.Shared.Enums
{
    public enum ClientReceivePacketEnum : ushort
    {
        CreateRoomResult = 1,
        NewRoomMessage,
        ChangeTitleRoomInfo,
        ChangeRoomInfo,
        RoomMemberJoinMessage,
        RoomMemberLeaveMessage,
        JoinRoomResult,
        LeaveRoomResult,
        ChatMessage,
        NewUserIdentity,
        RoomStartedMessage,
        RoomRemoveMessage
    }
}
