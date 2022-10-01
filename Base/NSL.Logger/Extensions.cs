using NSL.Logger.Interface;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Logger
{
    public static class Extensions
    {

        public static void AppendLog(this ILogger logger, string text)
        {
            logger.Append(LoggerLevel.Log, text);
        }

        public static void AppendDebug(this ILogger logger, string text)
        {
            logger.Append(LoggerLevel.Debug, text);
        }

        public static void AppendError(this ILogger logger, string text)
        {
            logger.Append(LoggerLevel.Error, text);
        }

        public static void AppendInfo(this ILogger logger, string text)
        {
            logger.Append(LoggerLevel.Info, text);
        }
    }
}
