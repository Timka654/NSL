using NSL.Logger.Info;
using NSL.Logger.Interface;
using SocketCore.Utils.Logger.Enums;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Utils;

namespace NSL.Logger
{
    public abstract class BaseLogger : ILogger, IDisposable
    {
        public bool Initialized { get; private set; }

        public string InstanceName { get; protected set; }

        private bool ConsoleOutput;

        private bool UnhandledCatch;

        private ConcurrentQueue<LogMessageInfo> WaitList;

        protected DateTime CurrentDateInitialized;

        protected int Delay;

        protected string FileTemplateName;

        private Timer outputTimer;

        internal bool Disponsed = false;

        public void AppendLog(string text)
        {
            Append(LoggerLevel.Log, text);
        }

        public void AppendDebug(string text)
        {
            Append(LoggerLevel.Debug, text);
        }

        public void AppendError(string text)
        {
            Append(LoggerLevel.Error, text);
        }

        public void AppendInfo(string text)
        {
            Append(LoggerLevel.Info, text);
        }

        public void Append(LoggerLevel level, string text)
        {
            if (!Initialized)
                return;

            var lm = new LogMessageInfo()
            {
                Now = DateTime.UtcNow,
                Level = level,
                Text = text
            };

            if (ConsoleOutput)
                ConsoleLogger.WriteLog(lm.Level, lm.ToString());

            WaitList.Enqueue(lm);
        }

        public void ConsoleLog(LoggerLevel level, string text)
        {
            if (ConsoleOutput)
                ConsoleLogger.WriteLog(level, $"[{level.ToString()}] - {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}: {text}");
        }

        protected void Initialize(string fileTemplateName, int delay)
        {
            FileTemplateName = fileTemplateName;

            Delay = delay;

            WaitList = new ConcurrentQueue<LogMessageInfo>();

            RunFlush();

            Initialized = true;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Flush();
        }

        private void RunFlush()
        {
            if (outputTimer != null)
                outputTimer.Dispose();

            outputTimer = new Timer((e) => Flush(), null, TimeSpan.FromMilliseconds(Delay), TimeSpan.FromMilliseconds(Delay));
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

        protected void FlushBuffer(Action<LogMessageInfo> processMessage)
        {
            while (WaitList.TryDequeue(out var message))
            {
                processMessage(message);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Append(LoggerLevel.Error, ((Exception)e.ExceptionObject).ToString());
            Flush();
        }

        public void Dispose()
        {
            if (Disponsed)
                return;

            Disponsed = true;

            outputTimer.Dispose();

            Flush();
            //LoggerStorage.DestroyLogger(InstanceName);
        }

        public abstract void Flush();
    }
}
