using SocketCore;

namespace SocketServer.Utils
{
    public interface INetworkListener
    {
        int GetListenerPort();

        void Start();

        void Stop();

        CoreOptions GetOptions();
    }

    public interface INetworkListener<TClient> : INetworkListener
        where TClient : IServerNetworkClient
    {
        ServerOptions<TClient> GetServerOptions();
    }
}
