using NSL.SocketServer.Utils;
using NSL.WebSockets.Server;

namespace NSL.BuilderExtensions.WebSocketsServer.AspNet
{
    public static class AspNetExtensions
    {
        public static AspNetWebSocketsServerEndPointBuilder<TClient, TOptions> AspWithOptions<TClient, TOptions>(this WebSocketsServerEndPointBuilder<TClient> builder)
            where TOptions : WSServerOptions<TClient>, new()
            where TClient : IServerNetworkClient, new()
        {
            return AspNetWebSocketsServerEndPointBuilder<TClient, TOptions>.Create();
        }
    }
}
