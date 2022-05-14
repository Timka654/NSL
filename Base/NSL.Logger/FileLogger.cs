using NSL.Logger.Info;
using NSL.Logger.Interface;
using SocketCore.Utils.Logger.Enums;
using System;
using System.IO;
using System.Threading;
using Utils;

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
            //if this method throwns - unlock after 2s
            if (!flushLocker.WaitOne(2_000))
                return;

            base.FlushBuffer(message =>
            {
                NextDay(message);

                stream.WriteLine(message.ToString());
            });

            stream?.Flush();

            flushLocker.Reset();
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
