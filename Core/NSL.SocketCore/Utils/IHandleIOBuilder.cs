using NSL.SocketCore;
using NSL.SocketCore.Utils;

namespace NSL.EndPointBuilder
{
    public interface IHandleIOBuilder<TClient>
        where TClient : BaseNetworkConnection
    {
        void AddReceiveHandle(CoreOptions.ReceivePacketHandle handle);

        void AddSendHandle(CoreOptions.SendPacketHandle handle);
    }
}
