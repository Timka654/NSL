using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Logger
{
    public class PerformanceLogger : BaseLogger
    {
        public static PerformanceLogger Initialize()
        {
            return LoggerStorage.InitializeLogger<PerformanceLogger>("performance", "performance", "performance", 5000);
        }

        public void AppendPerformance(string filename, string methodname, TimeSpan time)
        {
            Append(LoggerLevel.Performance, $"[{DateTime.Now}]\t[{filename}]\t[{methodname}]\t{time.TotalMilliseconds}\tms.");
        }
    }
}