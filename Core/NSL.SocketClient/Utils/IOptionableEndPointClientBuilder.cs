using NSL.EndPointBuilder;

namespace NSL.SocketClient.Utils
{
    public interface IOptionableEndPointClientBuilder<TClient> : IOptionableEndPointBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        ClientOptions<TClient> GetOptions();
    }
}
