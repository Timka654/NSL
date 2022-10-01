using NSL.BuilderExtensions.WebSockets;
using NSL.EndPointBuilder;
using NSL.SocketCore;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.WebSockets.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NSL.BuilderExtensions.WebSocketsServer
{
    public class WebSocketsServerEndPointBuilder : WebSocketsEndPointBuilder
    {
        private WebSocketsServerEndPointBuilder() { }

        public static WebSocketsServerEndPointBuilder Create()
        {
            return new WebSocketsServerEndPointBuilder();
        }

        public WebSocketsServerEndPointBuilder<TClient> WithClientProcessor<TClient>()
            where TClient : IServerNetworkClient, new()
        {
            return WebSocketsServerEndPointBuilder<TClient>.Create();
        }
    }

    public class WebSocketsServerEndPointBuilder<TClient> : WebSocketsEndPointBuilder
        where TClient : IServerNetworkClient, new()
    {
        private WebSocketsServerEndPointBuilder() { }

        public static WebSocketsServerEndPointBuilder<TClient> Create()
        {
            return new WebSocketsServerEndPointBuilder<TClient>();
        }

        public WebSocketsServerEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : WSServerOptions<TClient>, new()
        {
            return WebSocketsServerEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class WebSocketsServerEndPointBuilder<TClient, TOptions> : WebSocketsEndPointBuilder, IOptionableEndPointServerBuilder<TClient>, IHandleIOBuilder<WSServerClient<TClient>>
        where TClient : IServerNetworkClient, new()
        where TOptions : WSServerOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        event ReceivePacketDebugInfo<WSServerClient<TClient>> OnReceiveHandles;

        event SendPacketDebugInfo<WSServerClient<TClient>> OnSendHandles;

        public ServerOptions<TClient> GetOptions() => options;

        public CoreOptions<TClient> GetCoreOptions() => options;

        private WebSocketsServerEndPointBuilder() { }

        public static WebSocketsServerEndPointBuilder<TClient, TOptions> Create()
        {
            return new WebSocketsServerEndPointBuilder<TClient, TOptions>();
        }

        public WebSocketsServerEndPointBuilder<TClient, TOptions> WithCode(Action<WebSocketsServerEndPointBuilder<TClient, TOptions>> code)
        {
            code(this);
            return this;
        }

        /// <summary>
        /// EndPoint must have bindings in format http(/s)://{bindingAddress}:{bindingPort}/
        /// WARNING!!! "0.0.0.0" unsupported, you can use "*" or "+"
        /// </summary>
        public WebSocketsServerEndPointBuilder<TClient, TOptions> WithBindingPoint(string bindingUrl)
        {
            options.EndPoints.Add(bindingUrl);
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

        public WSServerListener<TClient> Build()
        {
            var result = new WSServerListener<TClient>(options);

            result.OnReceivePacket += OnReceiveHandles;

            result.OnSendPacket += OnSendHandles;

            return result;
        }
    }
}
