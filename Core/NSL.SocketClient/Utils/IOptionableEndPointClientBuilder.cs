using NSL.EndPointBuilder;

namespace NSL.SocketClient.Utils
{
    public interface IOptionableEndPointClientBuilder<TClient> : IOptionableEndPointBuilder<TClient>
        where TClient : BaseSocketNetworkClient, new()
    {
        ClientOptions<TClient> GetOptions();
    }
}
