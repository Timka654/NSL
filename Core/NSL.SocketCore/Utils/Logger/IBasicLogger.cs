using NSL.SocketCore.Utils.Logger.Enums;

namespace NSL.SocketCore.Utils.Logger
{
    public interface IBasicLogger
    {
        void Append(LoggerLevel level, string text);
    }
}
