using NSL.Logger.Info;
using NSL.Logger.Interface;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NSL.Logger
{
    public abstract class BaseLogger : ILogger, IDisposable
    {
        private bool ConsoleOutput;

        private bool UnhandledCatch;

        protected DateTime CurrentDateInitialized;

        protected bool Disposed { get; private set; } = false;

        protected Channel<LogMessageInfo> LogChannel = Channel.CreateUnbounded<LogMessageInfo>();

        public async void Append(LoggerLevel level, string text)
        {
            var lm = new LogMessageInfo()
            {
                Now = DateTime.UtcNow,
                Level = level,
                Text = text
            };

            if (ConsoleOutput)
                ConsoleLogger.WriteLog(lm.Level, lm.ToString());

            try { await LogChannel.Writer.WriteAsync(lm); } catch (InvalidOperationException) { }
        }

        public void ConsoleLog(LoggerLevel level, string text)
        {
            if (ConsoleOutput)
                ConsoleLogger.WriteLog(level, $"[{level.ToString()}] - {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}: {text}");
        }

        protected BaseLogger()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Flush();
        }

        public void SetConsoleOutput(bool allow)
        {
            ConsoleOutput = allow;
        }

        public void SetUnhandledExCatch(bool allow)
        {
            if (allow && !UnhandledCatch)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                UnhandledCatch = true;
            }
            else if (!allow && UnhandledCatch)
            {
                AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
                UnhandledCatch = false;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Append(LoggerLevel.Error, ((Exception)e.ExceptionObject).ToString());
            Flush();
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;

            LogChannel.Writer.Complete();

            Flush();

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
        }

        public virtual void Flush()
        {
            Task.Run(async () =>
            {
                while (LogChannel.Reader.TryPeek(out _))
                    await Task.Delay(100);

            }).Wait();
        }

    }
}
