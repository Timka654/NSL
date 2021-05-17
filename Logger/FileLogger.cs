using SocketCore.Utils.Logger.Enums;
using System;

namespace SCLogger
{
    public class FileLogger : BaseLogger
    {
        public static FileLogger Initialize()
        {
            return Initialize("logs");
        }

        public static FileLogger Initialize(string logsDir, string fileName = "log {date}",  int delayOutput = 5000, string instanceName = "{auto}")
        {
            if (instanceName == "{auto}")
                instanceName = Guid.NewGuid().ToString();

            FileLogger fl = LoggerStorage.InitializeLogger<FileLogger>(instanceName, fileName, logsDir, delayOutput);

            fl.InstanceName = instanceName;

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
