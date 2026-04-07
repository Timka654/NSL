using NSL.SocketCore.Utils;
using NSL.SocketCore;

namespace NSL.EndPointBuilder
{
    public interface IOptionableEndPointBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        CoreOptions<TClient> GetCoreOptions();
    }
}
