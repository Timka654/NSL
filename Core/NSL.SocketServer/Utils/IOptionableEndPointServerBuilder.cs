using NSL.EndPointBuilder;

namespace NSL.SocketServer.Utils
{
    public interface IOptionableEndPointServerBuilder<TClient> : IOptionableEndPointBuilder<TClient>
            where TClient : IServerNetworkClient, new()
    {
        ServerOptions<TClient> GetOptions();
    }
}
