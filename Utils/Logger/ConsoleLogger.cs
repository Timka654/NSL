using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Logger
{
    public class ConsoleLogger
    {
        private static object _locked = new object();
        public static void WriteFormat(LoggerLevel level, string text)
        {
            string r = $"[{level.ToString()}] - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {text}";
            WriteLog(level,r);
        }

        public static async void WriteLog(LoggerLevel level, string text)
        {
            await Task.Run(() =>
            {
                lock (_locked)
                {
                    switch (level)
                    {
                        case LoggerLevel.Info:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case LoggerLevel.Error:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case LoggerLevel.Log:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case LoggerLevel.Debug:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        default:
                            break;
                    }
                    Console.WriteLine(text);
                    FileLogger.WriteLog(text);
                }
            });
        }
    }
}
