using NSL.Logger.Info;
using System.IO;
using System.Threading;
using NSL.Utils;

namespace NSL.Logger
{
    public class FileLogger : BaseLogger
    {
        public FileLogger(string logsDir, string fileTemplateName = "log {date}", int delay = 5000, bool consoleOutput = true, bool handleUnhandledThrow = false)
        {
            LogsPath = logsDir;

            base.Initialize(fileTemplateName, delay);

            SetUnhandledExCatch(handleUnhandledThrow);
            SetConsoleOutput(consoleOutput);

        }

        /// <summary>
        /// Create instance with parameters 
        /// logsDir = "logs"
        /// handleUnhandledThrow = true
        /// </summary>
        public FileLogger() : this("logs", handleUnhandledThrow: true)
        {
        }

        private AutoResetEvent flushLocker = new AutoResetEvent(true);

        public override void Flush()
        {
            if (!flushLocker.WaitOne(2_000))
                return;

            try
            {
                base.FlushBuffer(message =>
                {
                    NextDay(message);

                    stream.WriteLine(message.ToString());
                });

                stream?.Flush();
            }
            catch (System.Exception ex)
            {
                ConsoleLog(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, ex.ToString());
            }

            flushLocker.Set();
        }

        private void NextDay(LogMessageInfo msg)
        {
            if (CurrentDateInitialized == msg.Now.Date)
                return;

            if (stream != null)
            {
                stream.Flush();
                stream.Close();
                stream = null;
            }

            IOUtils.CreateDirectoryIfNoExists(LogsPath);

            CurrentDateInitialized = msg.Now.Date;

            stream = new StreamWriter(Path.Combine(LogsPath, $"{FileTemplateName.Replace("{date}", CurrentDateInitialized.ToString("yyyy-MM-dd"))}.log"), true);

            stream.Flush();
        }

        protected TextWriter stream;

        protected string LogsPath;
    }
}
