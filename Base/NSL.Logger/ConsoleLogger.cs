using NSL.Logger.Interface;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Reflection.Emit;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NSL.Logger
{
    public class ConsoleLogger : ILogger, IDisposable
    {
        static Channel<(LoggerLevel level, string text)> writeChannel = Channel.CreateUnbounded<(LoggerLevel level, string text)>();

        internal static async void WriteLog(LoggerLevel level, string text)
        {
            await writeChannel.Writer.WriteAsync((level, text));

        }

        public void Append(LoggerLevel level, string text)
        {
            WriteLog(level, text);
        }

        public void ConsoleLog(LoggerLevel level, string text)
        {
            Append(level, text);
        }

        public void Flush()
        {
        }

        public void SetConsoleOutput(bool allow)
        {
        }

        public void SetUnhandledExCatch(bool allow)
        {
        }

        public static ConsoleLogger Create()
            => new ConsoleLogger();

        public ConsoleLogger()
        {
            processing();
        }

        private async void processing()
        {
            while (!disposed)
            {
                var item = await writeChannel.Reader.ReadAsync();

                switch (item.level)
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
                Console.WriteLine(item.text);
#if DEBUG
            System.Diagnostics.Debug.WriteLine(item.text);
#endif
            }
        }

        private bool disposed = false;

        public void Dispose()
        {
            if (disposed)
                return;

            writeChannel.Writer.Complete();
            disposed = true;
        }
    }
}
