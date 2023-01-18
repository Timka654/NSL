using NSL.Logger.Interface;
using NSL.SocketCore.Utils.Logger.Enums;

namespace NSL.Logger
{
    public class PrefixableLoggerProxy : ILogger
    {
        private readonly ILogger origin;
        private readonly string prefix;

        public PrefixableLoggerProxy(ILogger origin, string prefix)
        {
            this.origin = origin;
            this.prefix = prefix;
        }

        public void Append(LoggerLevel level, string text)
            => origin.Append(level, $"{prefix} {text}");

        public void ConsoleLog(LoggerLevel level, string text)
            => origin.ConsoleLog(level, $"{prefix} {text}");

        public void Flush()
            => origin.Flush();

        public void SetConsoleOutput(bool allow)
            => origin.SetConsoleOutput(allow);

        public void SetUnhandledExCatch(bool allow)
            => origin.SetUnhandledExCatch(allow);
    }
}
