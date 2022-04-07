using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.Enums
{
    public enum SignStatusCodeEnum : byte
    {
        ErrorPath,
        ErrorSession,
        Ok = byte.MaxValue
    }
}
