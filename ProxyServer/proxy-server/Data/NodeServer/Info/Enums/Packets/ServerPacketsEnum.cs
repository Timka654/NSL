using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ps.Data.NodeServer.Info.Enums.Packets
{
    /// <summary>
    /// Пакеты которые принимает сервер
    /// </summary>
    public enum ServerPacketsEnum : ushort
    {
        SignIn = 1,
        Transport
    }
}
