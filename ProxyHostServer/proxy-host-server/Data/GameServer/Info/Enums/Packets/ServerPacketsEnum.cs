using System;
using System.Collections.Generic;
using System.Text;

namespace phs.Data.GameServer.Info.Enums.Packets
{
    /// <summary>
    /// Пакеты которые принимает сервер
    /// </summary>
    public enum ServerPacketsEnum
    {
        SignIn = 1,
        PlayerConnected,
        PlayerDisconnected,
        PlayerDisconnectedError,
        PlayerInfo,
        MapInfoData,
        ServerInfoData,
        RoomInfo,
        ConfigurationData,
    }
}
