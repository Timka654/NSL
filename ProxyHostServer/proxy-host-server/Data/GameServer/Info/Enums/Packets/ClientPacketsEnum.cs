using System;
using System.Collections.Generic;
using System.Text;

namespace phs.Data.GameServer.Info.Enums.Packets
{
    /// <summary>
    /// Пакеты которые принимает клиент
    /// </summary>
    public enum ClientPacketsEnum
    {
        SignInResult = 1,
        PlayerConnected,
        PlayerDisconnected,
    }
}
