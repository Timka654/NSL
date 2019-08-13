using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phs.Data.NodeHostServer.Info.Enums.Packets
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
