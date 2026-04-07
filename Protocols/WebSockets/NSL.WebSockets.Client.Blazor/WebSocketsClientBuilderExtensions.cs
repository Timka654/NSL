using NSL.SocketClient;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Client;
using NSL.WebSockets.Client.Blazor;

namespace NSL.BuilderExtensions.WebSocketsClient.Blazor
{
    public static class WebSocketsClientBuilderExtensions
    {
        public static BlazorWSNetworkClient<TClient> BuildForBlazorWASMPlatform<TClient, TOptions>(this WebSocketsClientEndPointBuilder<TClient, TOptions> builder)
        where TClient : BaseNetworkConnection, new()
        where TOptions : WSClientOptions<TClient>, new()
            => new BlazorWSNetworkClient<TClient>(builder.GetWSClientOptions());
    }
}
