using NSL.SocketCore.Utils.Logger;
using NSL.SocketCore.Utils.Logger.Enums;

namespace NSL.Logger.Interface
{
    public interface ILogger : IBasicLogger
    {
        void ConsoleLog(LoggerLevel level, string text);
        void Flush();
        void SetConsoleOutput(bool allow);
        void SetUnhandledExCatch(bool allow);
    }
}
