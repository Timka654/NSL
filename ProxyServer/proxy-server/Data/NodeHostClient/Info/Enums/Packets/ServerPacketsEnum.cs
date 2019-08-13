using System;
using System.Collections.Generic;
using System.Text;

namespace ps.Data.NodeHostClient.Info.Enums.Packets
{
    /// <summary>
    /// Пакеты которые принимает сервер
    /// </summary>
    public enum ServerPacketsEnum
    {
        SignIn = 1,
        PlayerConnected,
        PlayerDisconnected,
    }
}
