using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace Logger
{
    internal class ConsoleLogger : DynamicObject
    {
        private static object _locked = new object();

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
                        case LoggerLevel.Performance:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        default:
                            break;
                    }
                    Console.WriteLine(text);
                }
            });
        }
    }
}
