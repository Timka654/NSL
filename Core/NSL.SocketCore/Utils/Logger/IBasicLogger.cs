using SocketCore.Utils.Logger.Enums;

namespace SocketCore.Utils.Logger
{
    public interface IBasicLogger
    {
        void Append(LoggerLevel level, string text);
    }
}
