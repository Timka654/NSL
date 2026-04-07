using NSL.SocketCore;
using NSL.SocketCore.Utils;

namespace NSL.SocketServer.Utils
{
    public interface INetworkListener
    {
        int GetListenerPort();

        void Start();

        void Stop();

        CoreOptions GetOptions();
    }

    public interface INetworkListener<TClient> : INetworkListener
        where TClient : BaseNetworkConnection
    {
        CoreOptions GetServerOptions();
    }
}
