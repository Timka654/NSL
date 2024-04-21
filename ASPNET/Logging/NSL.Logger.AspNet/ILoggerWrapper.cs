using Microsoft.Extensions.Logging;
using NSL.SocketCore.Utils.Logger.Enums;

namespace NSL.Logger.AspNet
{
    public class ILoggerWrapper : Logger.Interface.ILogger
    {
        private readonly ILogger originLogger;

        public ILoggerWrapper(ILogger originLogger)
        {
            this.originLogger = originLogger;
        }

        public void Append(LoggerLevel level, string text)
        {
            switch (level)
            {
                case LoggerLevel.Log:
                case LoggerLevel.Info:
                case LoggerLevel.Performance:
                    originLogger.LogInformation(text);
                    break;
                case LoggerLevel.Error:
                    originLogger.LogError(text);
                    break;
                case LoggerLevel.Debug:
                    originLogger.LogDebug(text);
                    break;
                default:
                    break;
            }
        }

        public void ConsoleLog(LoggerLevel level, string text)
        {
            throw new System.NotImplementedException();
        }

        public void Flush()
        {
            throw new System.NotImplementedException();
        }

        public void SetConsoleOutput(bool allow)
        {
            throw new System.NotImplementedException();
        }

        public void SetUnhandledExCatch(bool allow)
        {
            throw new System.NotImplementedException();
        }
    }
}