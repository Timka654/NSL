using NSL.SocketCore;
using NSL.SocketCore.Utils;

namespace NSL.EndPointBuilder
{
    public interface IHandleIOBuilder<TClient>
        where TClient : INetworkClient
    {
        void AddReceiveHandle(CoreOptions<TClient>.ReceivePacketHandle handle);

        void AddSendHandle(CoreOptions<TClient>.SendPacketHandle handle);
    }
}
