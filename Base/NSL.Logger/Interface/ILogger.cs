using SocketCore.Utils.Logger;
using SocketCore.Utils.Logger.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Logger.Interface
{
    public interface ILogger : IBasicLogger
    {
        void AppendDebug(string text);
        void AppendError(string text);
        void AppendInfo(string text);
        void AppendLog(string text);
        void ConsoleLog(LoggerLevel level, string text);
        void Flush();
        void SetConsoleOutput(bool allow);
        void SetUnhandledExCatch(bool allow);
    }
}
