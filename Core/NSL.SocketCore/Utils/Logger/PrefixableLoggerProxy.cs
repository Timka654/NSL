using NSL.SocketCore.Utils.Logger;
using NSL.SocketCore.Utils.Logger.Enums;

namespace NSL.Logger
{
    public class PrefixableLoggerProxy : IBasicLogger
    {
        private readonly IBasicLogger origin;
        private readonly string prefix;

        public PrefixableLoggerProxy(IBasicLogger origin, string prefix)
        {
            this.origin = origin;
            this.prefix = prefix;
        }

        public void Append(LoggerLevel level, string text)
            => origin.Append(level, $"{prefix} {text}");
    }
}
