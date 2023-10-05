using NSL.SocketClient;
using NSL.WebSockets.Client;
using NSL.WebSockets.UnityClient;

namespace NSL.BuilderExtensions.WebSocketsClient.Unity
{
    public static class WebSocketsClientBuilderExtensions
    {
        public static WGLWSNetworkClient<TClient, TOptions> BuildForWGLPlatform<TClient, TOptions>(this WebSocketsClientEndPointBuilder<TClient, TOptions> builder)
        where TClient : BaseSocketNetworkClient, new()
        where TOptions : WSClientOptions<TClient>, new()
            => new WGLWSNetworkClient<TClient, TOptions>(builder.GetWSClientOptions());
    }
}