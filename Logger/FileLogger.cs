using SocketCore.Utils.Logger.Enums;

namespace SCLogger
{
    public class FileLogger : BaseLogger
    {
        public static FileLogger Initialize()
        {
            return Initialize("logs");
        }

        public static FileLogger Initialize(string logsDir, string fileName = "log",  int delayOutput = 5000, string nameInStorage = "main")
        {
            FileLogger fl = LoggerStorage.InitializeLogger<FileLogger>(nameInStorage, fileName, logsDir, delayOutput);

            fl.SetUnhandledExCatch(true);
            fl.SetConsoleOutput(true);

            return fl;
        }

        public void AppendLog(string text)
        {
            base.Append(LoggerLevel.Log, text);
        }

        public void AppendDebug(string text)
        {
            base.Append(LoggerLevel.Debug, text);
        }

        public void AppendError(string text)
        {
            base.Append(LoggerLevel.Error, text);
        }

        public void AppendInfo(string text)
        {
            base.Append(LoggerLevel.Info, text);
        }

        public new void Flush()
        {
            base.Flush();
        }
    }
}
