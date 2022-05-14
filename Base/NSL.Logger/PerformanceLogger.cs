using NSL.Logger.Interface;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NSL.Logger
{
    public class PerformanceLogger : ILogger
    {
        BaseLogger outputLogger;

        /// <summary>
        /// Create FileLogger with parameters 
        /// </summary>
        /// <param name="logsDir"></param>
        /// <param name="fileTemplateName"></param>
        /// <param name="delay"></param>
        /// <param name="consoleOutput"></param>
        public PerformanceLogger(string logsDir, string fileTemplateName = "performance {date}", int delay = 5000, bool consoleOutput = true)
            : this(new FileLogger(logsDir, fileTemplateName, delay, consoleOutput, false))
        {
        }

        /// <summary>
        /// Create instance with parameters 
        /// logsDir = "logs/performance"
        /// handleUnhandledThrow = true
        /// </summary>
        public PerformanceLogger()
            : this(Path.Combine("logs", "performance"))
        {

        }

        public PerformanceLogger(BaseLogger outputLogger)
        {
            this.outputLogger = outputLogger;
        }

        public void ProcessPerformance(string fileName, string methodName, Action performAction)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();

            performAction();

            sw.Stop();

            AppendPerformance(fileName, methodName, sw.Elapsed);
        }

        public async Task ProcessPerformance(string fileName, string methodName, Func<Task> performAction)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();

            await performAction();

            sw.Stop();

            AppendPerformance(fileName, methodName, sw.Elapsed);
        }

        public void AppendPerformance(string fileName, string methodName, TimeSpan time)
        {
            Append(LoggerLevel.Performance, $"[{DateTime.UtcNow}]\t[{fileName}]\t[{methodName}]\t{time.TotalMilliseconds}\tms.");
        }

        public void Append(LoggerLevel level, string text)
        {
            if (level != LoggerLevel.Performance)
                throw new NotSupportedException();

            outputLogger.Append(level, text);
        }

        public void AppendDebug(string text)
        {
            outputLogger.Append(LoggerLevel.Debug, text);
        }

        public void AppendError(string text)
        {
            throw new NotSupportedException();
        }

        public void AppendInfo(string text)
        {
            throw new NotSupportedException();
        }

        public void AppendLog(string text)
        {
            throw new NotSupportedException();
        }

        public void ConsoleLog(LoggerLevel level, string text)
        {
            throw new NotSupportedException();
        }

        public void Flush()
        {
            outputLogger.Flush();
        }

        public void SetConsoleOutput(bool allow)
        {
            outputLogger.SetConsoleOutput(allow);
        }

        public void SetUnhandledExCatch(bool allow)
        {
            throw new NotSupportedException();
        }
    }
}