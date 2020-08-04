using SocketCore.Utils.Logger.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketCore.Utils.Logger
{
    public interface IBasicLogger
    {
        void Append(LoggerLevel level, string text);
    }
}
