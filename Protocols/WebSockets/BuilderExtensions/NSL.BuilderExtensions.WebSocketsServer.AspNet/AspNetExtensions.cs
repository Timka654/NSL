using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NSL.WebSockets.Server;
using NSL.BuilderExtensions.SocketCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NSL.WebSockets.Server.AspNetPoint;

namespace NSL.BuilderExtensions.WebSocketsServer.AspNet
{
    public static class AspNetExtensions
    {
        /// <summary>
        /// Previously call UseWebSockets method for normal work
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pattern"></param>
        /// <param name="configure"></param>
        /// <param name="requestHandle"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapWebSocketsPoint(this IEndpointRouteBuilder builder, string pattern, Action<AspNetWebSocketsServerEndPointBuilder<AspNetWSNetworkServerClient, WSServerOptions<AspNetWSNetworkServerClient>>> configure = null,
            Func<HttpContext, Task<bool>> requestHandle = null)
            => MapWebSocketsPoint<AspNetWSNetworkServerClient>(builder, pattern, configure, requestHandle);

        /// Previously call UseWebSockets method for normal work
        public static IEndpointConventionBuilder MapWebSocketsPoint<TClient>(
            this IEndpointRouteBuilder builder,
            string pattern,
            Action<AspNetWebSocketsServerEndPointBuilder<TClient, WSServerOptions<TClient>>> configure = null,
            Func<HttpContext, Task<bool>> requestHandle = null)
            where TClient : AspNetWSNetworkServerClient, new()
        {
            var wsBuilder = AspNetWebSocketsServerEndPointBuilder<TClient, WSServerOptions<TClient>>.Create();

            wsBuilder.WithBufferSize(8192);

            wsBuilder.WithBindingPoint(pattern);

            if (configure != null)
                configure(wsBuilder);

            var server = wsBuilder.BuildWithoutRoute();

            var acceptDelegate = server.GetAcceptDelegate();

            return builder.MapGet(pattern, async context =>
            {
                if (requestHandle != null)
                    if (!await requestHandle(context))
                        return;

                await acceptDelegate(context);
            });
        }

        public static AspNetWebSocketsServerEndPointBuilder<TClient, WSServerOptions<TClient>> AspWithOptions<TClient>(this WebSocketsServerEndPointBuilder<TClient> builder)
            where TClient : AspNetWSNetworkServerClient, new()
            => builder.AspWithOptions<TClient, WSServerOptions<TClient>>();

        public static AspNetWebSocketsServerEndPointBuilder<TClient, TOptions> AspWithOptions<TClient, TOptions>(this WebSocketsServerEndPointBuilder<TClient> builder)
            where TOptions : WSServerOptions<TClient>, new()
            where TClient : AspNetWSNetworkServerClient, new()
        {
            return AspNetWebSocketsServerEndPointBuilder<TClient, TOptions>.Create();
        }
    }
}
