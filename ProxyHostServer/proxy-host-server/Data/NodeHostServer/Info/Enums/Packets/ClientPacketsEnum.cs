using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phs.Data.NodeHostServer.Info.Enums.Packets
{
    /// <summary>
    /// Пакеты которые принимает клиент
    /// </summary>
    public enum ClientPacketsEnum
    {
        SignInResult = 1,
        PlayerConnectedResult
    }
}
