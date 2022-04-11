using SocketCore.Utils.Logger.Enums;
using System;

namespace NSL.Logger
{
    public class PerformanceLogger : BaseLogger
    {
        public static PerformanceLogger Initialize()
        {
            return LoggerStorage.InitializeLogger<PerformanceLogger>("performance {date}", "performance", "performance", 5000);
        }

        public void AppendPerformance(string filename, string methodname, TimeSpan time)
        {
            Append(LoggerLevel.Performance, $"[{DateTime.UtcNow}]\t[{filename}]\t[{methodname}]\t{time.TotalMilliseconds}\tms.");
        }
    }
}