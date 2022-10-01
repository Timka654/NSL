using NSL.SocketCore.Utils;
using NSL.SocketCore;
using NSL.Utils;

namespace NSL.EndPointBuilder
{
    public interface IOptionableEndPointBuilder<TClient> : IEndPointBuilder
        where TClient : INetworkClient, new()
    {
        CoreOptions<TClient> GetCoreOptions();
    }
}
