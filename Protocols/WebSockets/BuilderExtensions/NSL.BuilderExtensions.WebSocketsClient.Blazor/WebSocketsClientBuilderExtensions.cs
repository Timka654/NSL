using NSL.SocketClient;
using NSL.WebSockets.Client;
using NSL.WebSockets.Client.Blazor;

namespace NSL.BuilderExtensions.WebSocketsClient.Blazor
{
    public static class WebSocketsClientBuilderExtensions
    {
        public static BlazorWSNetworkClient<TClient, TOptions> BuildForBlazorWASMPlatform<TClient, TOptions>(this WebSocketsClientEndPointBuilder<TClient, TOptions> builder)
        where TClient : BaseSocketNetworkClient, new()
        where TOptions : WSClientOptions<TClient>, new()
        {
            var result = new BlazorWSNetworkClient<TClient, TOptions>(builder.GetWSClientOptions());

            result.OnReceivePacket += builder.GetReceiveHandles();

            result.OnSendPacket += builder.GetSendHandles();

            return result;
        }
    }
}
