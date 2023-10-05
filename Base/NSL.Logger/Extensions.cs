using NSL.Logger.Interface;
using NSL.SocketCore.Utils.Logger;
using NSL.SocketCore.Utils.Logger.Enums;

namespace NSL.Logger
{
    public static class Extensions
    {
        public static void AppendLog(this IBasicLogger logger, string text)
        {
            logger.Append(LoggerLevel.Log, text);
        }

        public static void AppendDebug(this IBasicLogger logger, string text)
        {
            logger.Append(LoggerLevel.Debug, text);
        }

        public static void AppendError(this IBasicLogger logger, string text)
        {
            logger.Append(LoggerLevel.Error, text);
        }

        public static void AppendInfo(this IBasicLogger logger, string text)
        {
            logger.Append(LoggerLevel.Info, text);
        }
    }
}
