using NSL.SocketCore.Utils;
using NSL.SocketCore;

namespace NSL.EndPointBuilder
{
    public interface IOptionableEndPointBuilder<TClient>
        where TClient : INetworkClient, new()
    {
        CoreOptions<TClient> GetCoreOptions();
    }
}
