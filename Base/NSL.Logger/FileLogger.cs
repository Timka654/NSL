using NSL.Logger.Info;
using System.IO;
using System.Threading;
using NSL.Utils;

namespace NSL.Logger
{
    public class FileLogger : BaseLogger
    {

        protected string FileTemplateName;

        public FileLogger(string logsDir, string fileTemplateName = "log {date}", bool consoleOutput = true, bool handleUnhandledThrow = false)
        {
            LogsPath = logsDir;

            FileTemplateName = fileTemplateName;

            SetUnhandledExCatch(handleUnhandledThrow);
            SetConsoleOutput(consoleOutput);

            processingLogs();
        }

        long n = 0;

        async void processingLogs()
        {
            var reader = LogChannel.Reader;

            while (!Disposed)
            {
                try
                {
                    await foreach (var message in reader.ReadAllAsync())
                    {
                        NextDay(message);

                        stream.WriteLine(message.ToString());

                        if (++n % 10 == 0)
                            stream.Flush();
                    }
                }
                catch (System.Exception ex)
                {
                    ConsoleLog(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Create instance with parameters 
        /// logsDir = "logs"
        /// handleUnhandledThrow = true
        /// </summary>
        public FileLogger() : this("logs", handleUnhandledThrow: true)
        {
        }

        public override void Flush()
        {
            base.Flush();
            stream.Flush();
        }

        private void NextDay(LogMessageInfo msg)
        {
            if (CurrentDateInitialized == msg.Now.Date)
                return;

            CurrentDateInitialized = msg.Now.Date;

            if (stream != null)
            {
                stream.Flush();
                stream.Close();
                stream = null;
            }

            IOUtils.CreateDirectoryIfNoExists(LogsPath);

            stream = new StreamWriter(Path.Combine(LogsPath, $"{FileTemplateName.Replace("{date}", CurrentDateInitialized.ToString("yyyy-MM-dd"))}.log"), true);

            stream.Flush();
        }

        protected TextWriter stream;

        protected string LogsPath;
    }
}
