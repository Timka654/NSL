using NSL.EndPointBuilder;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using NSL.WebSockets.Server;
using System;
using Microsoft.AspNetCore.Routing;
using NSL.WebSockets.Server.AspNetPoint;

namespace NSL.BuilderExtensions.WebSocketsServer.AspNet
{
    public class AspNetWebSocketsServerEndPointBuilder<TClient, TOptions> : IOptionableEndPointBuilder, IHandleIOBuilder<TClient>
            where TClient : AspNetWSNetworkServerClient, new()
            where TOptions : WSServerOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        public CoreOptions GetOptions() => options;

        public CoreOptions GetCoreOptions() => options;

        private AspNetWebSocketsServerEndPointBuilder() { }

        public static AspNetWebSocketsServerEndPointBuilder<TClient, TOptions> Create()
        {
            return new AspNetWebSocketsServerEndPointBuilder<TClient, TOptions>();
        }

        public AspNetWebSocketsServerEndPointBuilder<TClient, TOptions> WithCode(Action<AspNetWebSocketsServerEndPointBuilder<TClient, TOptions>> code)
        {
            code(this);
            return this;
        }

        /// <summary>
        /// relativeUrl
        /// </summary>
        public AspNetWebSocketsServerEndPointBuilder<TClient, TOptions> WithBindingPoint(string pattern)
        {
            options.EndPoints.Add(pattern);
            return this;
        }

        public void AddReceiveHandle(CoreOptions.ReceivePacketHandle handle)
        {
            options.OnReceivePacket += handle;
        }

        public void AddSendHandle(CoreOptions.SendPacketHandle handle)
        {
            options.OnSendPacket += handle;
        }

        public AspNetWebSocketsServer<TClient> Build(IEndpointRouteBuilder router)
            => new AspNetWebSocketsServer<TClient>(router, options);

        public AspNetWebSocketsServer<TClient> BuildWithoutRoute()
            => new AspNetWebSocketsServer<TClient>(options);
    }
}

