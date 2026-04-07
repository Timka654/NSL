using NSL.EndPointBuilder;
using NSL.SocketCore;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.WebSockets.Server;
using System;

namespace NSL.BuilderExtensions.WebSocketsServer
{
    public class WebSocketsServerEndPointBuilder
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

    public class WebSocketsServerEndPointBuilder<TClient>
        where TClient : IServerNetworkClient, new()
    {
        private WebSocketsServerEndPointBuilder() { }

        public static WebSocketsServerEndPointBuilder<TClient> Create()
        {
            return new WebSocketsServerEndPointBuilder<TClient>();
        }

        public WebSocketsServerEndPointBuilder<TClient, WSServerOptions<TClient>> WithOptions()
            => WithOptions<WSServerOptions<TClient>>();

        public WebSocketsServerEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : WSServerOptions<TClient>, new()
        {
            return WebSocketsServerEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class WebSocketsServerEndPointBuilder<TClient, TOptions> : IOptionableEndPointServerBuilder<TClient>, IHandleIOBuilder<TClient>
        where TClient : IServerNetworkClient, new()
        where TOptions : WSServerOptions<TClient>, new()
    {
        TOptions options = new TOptions();

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

        public void AddReceiveHandle(CoreOptions<TClient>.ReceivePacketHandle handle)
        {
            options.OnReceivePacket += handle;
        }

        public void AddSendHandle(CoreOptions<TClient>.SendPacketHandle handle)
        {
            options.OnSendPacket += handle;
        }

        public WSServerListener<TClient> Build()
            => new WSServerListener<TClient>(options);
    }
}
