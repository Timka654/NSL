using NSL.EndPointBuilder;

namespace NSL.SocketServer.Utils
{
    public interface IOptionableEndPointServerBuilder<TClient> : IOptionableEndPointBuilder<TClient>
            where TClient : BaseNetworkConnection, new()
    {
        ServerOptions<TClient> GetOptions();
    }
}
