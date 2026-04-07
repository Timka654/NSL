using NSL.EndPointBuilder;
using NSL.LocalBridge;
using NSL.SocketCore.Utils;
using System.Net;

namespace NSL.BuilderExtensions.LocalBridge
{
    public static class Extensions
    {
        public static LocalBridgeClient<TClient, TAnotherClient> CreateLocalBridge<TClient, TAnotherClient>(this IOptionableEndPointBuilder builder, IPEndPoint connectionEndPoint = null, LocalBridgeClient<TAnotherClient, TClient> anotherClient = null)
            where TClient : BaseNetworkConnection, new()
            where TAnotherClient : BaseNetworkConnection, new()
        {
            return new LocalBridgeClient<TClient, TAnotherClient>(builder.GetCoreOptions(), connectionEndPoint, anotherClient);
        }
    }
}