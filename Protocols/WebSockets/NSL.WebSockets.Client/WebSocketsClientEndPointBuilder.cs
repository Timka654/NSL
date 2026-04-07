using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore;
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
            where TClient : BaseSocketNetworkClient, new()
        {
            return WebSocketsClientEndPointBuilder<TClient>.Create();
        }
    }

    public class WebSocketsClientEndPointBuilder<TClient>
        where TClient : BaseSocketNetworkClient, new()
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

    public class WebSocketsClientEndPointBuilder<TClient, TOptions> : IOptionableEndPointClientBuilder<TClient>, IHandleIOBuilder<TClient>
        where TClient : BaseSocketNetworkClient, new()
        where TOptions : WSClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        public ClientOptions<TClient> GetOptions() => options;

        public CoreOptions<TClient> GetCoreOptions() => options;

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

        public void AddReceiveHandle(CoreOptions<TClient>.ReceivePacketHandle handle)
        {
            options.OnReceivePacket += handle;
        }

        public void AddSendHandle(CoreOptions<TClient>.SendPacketHandle handle)
        {
            options.OnSendPacket += handle;
        }

        public WSNetworkClient<TClient, TOptions> Build()
            => new WSNetworkClient<TClient, TOptions>(options);
    }
}
