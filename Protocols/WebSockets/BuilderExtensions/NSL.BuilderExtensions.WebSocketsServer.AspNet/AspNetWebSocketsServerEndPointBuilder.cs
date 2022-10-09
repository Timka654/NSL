using NSL.BuilderExtensions.WebSockets;
using NSL.EndPointBuilder;
using NSL.SocketCore;
using NSL.SocketServer.Utils;
using NSL.SocketServer;
using NSL.WebSockets.Server;
using System;
using Microsoft.AspNetCore.Routing;
using NSL.WebSockets.Server.AspNetPoint;

namespace NSL.BuilderExtensions.WebSocketsServer.AspNet
{
    public class AspNetWebSocketsServerEndPointBuilder<TClient, TOptions> : WebSocketsEndPointBuilder, IOptionableEndPointServerBuilder<TClient>, IHandleIOBuilder<WSServerClient<TClient>>
            where TClient : AspNetWSNetworkServerClient, new()
            where TOptions : WSServerOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        event ReceivePacketDebugInfo<WSServerClient<TClient>> OnReceiveHandles;

        event SendPacketDebugInfo<WSServerClient<TClient>> OnSendHandles;

        public ServerOptions<TClient> GetOptions() => options;

        public CoreOptions<TClient> GetCoreOptions() => options;

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

        public void AddReceiveHandle(ReceivePacketDebugInfo<WSServerClient<TClient>> handle)
        {
            OnReceiveHandles += handle;
        }

        public void AddSendHandle(SendPacketDebugInfo<WSServerClient<TClient>> handle)
        {
            OnSendHandles += handle;
        }

        public void AddBaseReceiveHandle(ReceivePacketDebugInfo<IClient> handle)
        {
            OnReceiveHandles += (client, pid, len) => handle(client, pid, len);
        }

        public void AddBaseSendHandle(SendPacketDebugInfo<IClient> handle)
        {
            OnSendHandles += (client, pid, len, stack) => handle(client, pid, len, stack);
        }

        public AspNetWebSocketsServer<TClient> Build(IEndpointRouteBuilder router)
        {
            var result = new AspNetWebSocketsServer<TClient>(router, options);

            result.OnReceivePacket += OnReceiveHandles;

            result.OnSendPacket += OnSendHandles;

            return result;
        }

        public AspNetWebSocketsServer<TClient> BuildWithoutRoute()
        {
            var result = new AspNetWebSocketsServer<TClient>(options);

            result.OnReceivePacket += OnReceiveHandles;

            result.OnSendPacket += OnSendHandles;

            return result;
        }
    }
}

