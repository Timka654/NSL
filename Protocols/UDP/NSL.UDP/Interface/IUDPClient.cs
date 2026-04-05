using NSL.SocketCore.Utils;

namespace NSL.UDP.Interface
{
    public interface IUDPClient
    {
        void Send(UDPChannelEnum channel, byte[] buffer);
    }
}
