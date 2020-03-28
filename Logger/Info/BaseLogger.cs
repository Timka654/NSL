﻿using Logger.Info;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Logger
{
    public class BaseLogger : ILogger
    {
        public bool Initialized { get; private set; }

        private bool ConsoleOutput;

        private bool UnhandledCatch;

        private ConcurrentQueue<LogMessageInfo> WaitList;

        protected DateTime CurrentDateInitialized;

        protected TextWriter stream;

        protected string LogsPath;

        protected int Delay;

        protected string FileName;


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

            if (!Directory.Exists(LogsPath))
                Directory.CreateDirectory(LogsPath);

            CurrentDateInitialized = msg.Now.Date;

            stream = new StreamWriter(Path.Combine(LogsPath, $"{FileName} {CurrentDateInitialized.ToString("dd-MM-yyyy")}.log"), true);

            stream.Flush();
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

        public virtual void Initialize(string fname, string path, int delay)
        {
            FileName = fname;

            LogsPath = path;

            Delay = delay;

            WaitList = new ConcurrentQueue<LogMessageInfo>();

            RunOutput();

            Initialized = true;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Flush();
        }

        private async void RunOutput()
        {
            while (true)
            {
                await Task.Delay(Delay);

                Flush();
            }
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

        protected void Flush()
        {
            while (WaitList.TryDequeue(out var message))
            {
                NextDay(message);

                stream.WriteLine(message.ToString());
            }
            stream?.Flush();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Append(LoggerLevel.Error, ((Exception)e.ExceptionObject).ToString());
            Flush();
        }
    }
}
