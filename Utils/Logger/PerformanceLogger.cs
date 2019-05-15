using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Logger
{
    public class PerformanceLogger
    {
        public static bool Initialized { get; private set; }

        private static Queue<string> WaitList;

        private static DateTime CurrentDate = DateTime.MinValue.Date;

        private static ManualResetEvent wait_list_locker = new ManualResetEvent(true);

        private static StreamWriter stream;

        private static int DelayTime = 5000;

        private static string LogsPath = "performance";

        public static void Initialize()
        {
            WaitList = new Queue<string>();

            Initialized = true;

            NextDay();

            Task.Run(FlushWhile);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ConsoleLogger.WriteFormat(LoggerLevel.Error, ((Exception)e.ExceptionObject).ToString());

            while (WaitList.Count > 0)
            {
                stream.WriteLine(WaitList.Dequeue());
            }

            stream.Flush();
        }

        private static void NextDay()
        {
            if (CurrentDate == DateTime.Now.Date)
                return;

            if (stream != null)
            {
                stream.Flush();
                stream.Close();
                stream = null;
            }

            if (!Directory.Exists(LogsPath))
                Directory.CreateDirectory(LogsPath);

            CurrentDate = DateTime.Now.Date;

            stream = new StreamWriter(Path.Combine(LogsPath, $"performance {CurrentDate:dd-MM-yyyy}.log"), true);
            
            ConsoleLogger.WriteFormat(LoggerLevel.Info, "Initialization Performance Logger");

            stream.Flush();
        }

        public static void WritePerformance(string filename, string methodname, TimeSpan time)
        {
            WritePerformance($"[{DateTime.Now}]\t[{filename}]\t[{methodname}]\t{time.TotalMilliseconds}\tms.");
        }

        public static void WritePerformance(string text)
        {
            if (!Initialized)
                return;
            wait_list_locker.WaitOne();
            WaitList.Enqueue(text);
        }

        private static readonly Action FlushWhile = new Action(async () =>
        {
            while (true)
            {
                await Task.Delay(DelayTime);
                wait_list_locker.Reset();
                try
                {

                    await Task.Delay(100);

                    while (WaitList.Count > 0)
                    {
                        stream.WriteLine(WaitList.Dequeue());
                    }
                    stream.Flush();
                }
                catch (Exception)
                {
                    throw;
                }
                wait_list_locker.Set();
                NextDay();
            }
        });
    }
}
