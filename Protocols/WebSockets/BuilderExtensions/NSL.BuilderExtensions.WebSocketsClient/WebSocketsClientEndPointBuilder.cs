using NSL.BuilderExtensions.WebSockets;
using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore;
using NSL.WebSockets.Client;
using System;

namespace NSL.BuilderExtensions.WebSocketsClient
{
    public class WebSocketsClientEndPointBuilder : WebSocketsEndPointBuilder
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

    public class WebSocketsClientEndPointBuilder<TClient> : WebSocketsEndPointBuilder
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

    public class WebSocketsClientEndPointBuilder<TClient, TOptions> : WebSocketsEndPointBuilder, IOptionableEndPointClientBuilder<TClient>, IHandleIOBuilder<WSClient<TClient>>
        where TClient : BaseSocketNetworkClient, new()
        where TOptions : WSClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        event ReceivePacketDebugInfo<WSClient<TClient>> OnReceiveHandles;

        event SendPacketDebugInfo<WSClient<TClient>> OnSendHandles;

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

        public void AddReceiveHandle(ReceivePacketDebugInfo<WSClient<TClient>> handle)
        {
            OnReceiveHandles += handle;
        }

        public void AddSendHandle(SendPacketDebugInfo<WSClient<TClient>> handle)
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

        public WSNetworkClient<TClient, TOptions> Build()
        {
            var result = new WSNetworkClient<TClient, TOptions>(options);

            result.OnReceivePacket += OnReceiveHandles;

            result.OnSendPacket += OnSendHandles;

            return result;
        }

        public ReceivePacketDebugInfo<WSClient<TClient>> GetReceiveHandles()
            => OnReceiveHandles;

        public SendPacketDebugInfo<WSClient<TClient>> GetSendHandles()
            => OnSendHandles;
    }
}
