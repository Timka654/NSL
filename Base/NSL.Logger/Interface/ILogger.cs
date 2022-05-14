using NSL.SocketCore.Utils.Logger;
using NSL.SocketCore.Utils.Logger.Enums;

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
