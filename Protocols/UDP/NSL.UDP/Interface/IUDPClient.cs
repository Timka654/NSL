using NSL.UDP.Enums;

namespace NSL.UDP.Interface
{
    public interface IUDPClient
    {
        void Send(UDPChannelEnum channel, byte[] buffer);
    }
}
