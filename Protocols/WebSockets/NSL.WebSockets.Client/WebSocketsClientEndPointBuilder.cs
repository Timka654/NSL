using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Client;
using System;

namespace NSL.BuilderExtensions.WebSocketsClient
{
    public class WebSocketsClientEndPointBuilder
    {
        private WebSocketsClientEndPointBuilder() { }

        public static WebSocketsClientEndPointBuilder Create()
        {
            return new WebSocketsClientEndPointBuilder();
        }

        public WebSocketsClientEndPointBuilder<TClient> WithClientProcessor<TClient>()
            where TClient : BaseNetworkConnection, new()
        {
            return WebSocketsClientEndPointBuilder<TClient>.Create();
        }
    }

    public class WebSocketsClientEndPointBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        private WebSocketsClientEndPointBuilder() { }

        public static WebSocketsClientEndPointBuilder<TClient> Create()
        {
            return new WebSocketsClientEndPointBuilder<TClient>();
        }

        public WebSocketsClientEndPointBuilder<TClient, WSClientOptions<TClient>> WithOptions()
            => WithOptions<WSClientOptions<TClient>>();

        public WebSocketsClientEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : WSClientOptions<TClient>, new()
        {
            return WebSocketsClientEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class WebSocketsClientEndPointBuilder<TClient, TOptions> : IOptionableEndPointBuilder, IHandleIOBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
        where TOptions : WSClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        public CoreOptions GetOptions() => options;

        public CoreOptions GetCoreOptions() => options;

        public TOptions GetWSClientOptions() => options;

        private WebSocketsClientEndPointBuilder() { }

        public static WebSocketsClientEndPointBuilder<TClient, TOptions> Create()
        {
            return new WebSocketsClientEndPointBuilder<TClient, TOptions>();
        }

        public WebSocketsClientEndPointBuilder<TClient, TOptions> WithCode(Action<WebSocketsClientEndPointBuilder<TClient, TOptions>> code)
        {
            code(this);
            return this;
        }

        public WebSocketsClientEndPointBuilder<TClient, TOptions> WithUrl(Uri url)
        {
            options.EndPoint = url;
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

        public WSNetworkClient<TClient> Build()
            => new WSNetworkClient<TClient>(options);
    }
}
