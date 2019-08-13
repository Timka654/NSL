using System;
using System.Collections.Generic;
using System.Text;

namespace ps.Data.NodeHostClient.Info.Enums.Packets
{
    /// <summary>
    /// Пакеты которые принимает клиент
    /// </summary>
    public enum ClientPacketsEnum
    {
        SignInResult = 1,
        PlayerConnectedResult,
        PlayerInfoResult,
        MapInfoDataResult,
        ServerInfoDataResult,
        RoomDestroy,
        PlayerDisconnected,
        RoomInfoResult,
        ConfigurationDataResult,
    }
}
