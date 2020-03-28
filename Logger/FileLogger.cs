namespace Logger
{
    public class FileLogger : BaseLogger
    {
        public static FileLogger Initialize()
        {
            FileLogger fl = LoggerStorage.InitializeLogger<FileLogger>("main", "log", "logs", 5000);

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
    }
}
