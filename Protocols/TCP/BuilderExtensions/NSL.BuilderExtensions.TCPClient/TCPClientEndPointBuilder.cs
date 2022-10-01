using NSL.BuilderExtensions.TCP;
using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore;
using NSL.TCP.Client;
using System;
using System.Net;

namespace NSL.BuilderExtensions.TCPClient
{
    public class TCPClientEndPointBuilder : TCPEndPointBuilder
    {
        private TCPClientEndPointBuilder() { }

        public static TCPClientEndPointBuilder Create()
        {
            return new TCPClientEndPointBuilder();
        }

        public TCPClientEndPointBuilder<TClient> WithClientProcessor<TClient>()
            where TClient : BaseSocketNetworkClient, new()
        {
            return TCPClientEndPointBuilder<TClient>.Create();
        }
    }

    public class TCPClientEndPointBuilder<TClient> : TCPEndPointBuilder
        where TClient : BaseSocketNetworkClient, new()
    {
        private TCPClientEndPointBuilder() { }

        public static TCPClientEndPointBuilder<TClient> Create()
        {
            return new TCPClientEndPointBuilder<TClient>();
        }

        public TCPClientEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : ClientOptions<TClient>, new()
        {
            return TCPClientEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class TCPClientEndPointBuilder<TClient, TOptions> : TCPEndPointBuilder, IOptionableEndPointClientBuilder<TClient>, IHandleIOBuilder<TCPClient<TClient>>
        where TClient : BaseSocketNetworkClient, new()
        where TOptions : ClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        event ReceivePacketDebugInfo<TCPClient<TClient>> OnReceiveHandles;

        event SendPacketDebugInfo<TCPClient<TClient>> OnSendHandles;

        public ClientOptions<TClient> GetOptions() => options;

        public CoreOptions<TClient> GetCoreOptions() => options;

        private TCPClientEndPointBuilder() { }

        public static TCPClientEndPointBuilder<TClient, TOptions> Create()
        {
            return new TCPClientEndPointBuilder<TClient, TOptions>();
        }

        public new TCPClientEndPointBuilder<TClient, TOptions> WithCode(Action<TCPClientEndPointBuilder<TClient, TOptions>> code)
        {
            code(this);
            return this;
        }

        public TCPClientEndPointBuilder<TClient, TOptions> WithEndPoint(IPEndPoint endpoint)
        {
            return WithEndPoint(endpoint.Address, endpoint.Port);
        }

        public TCPClientEndPointBuilder<TClient, TOptions> WithEndPoint(IPAddress ip, int port)
        {
            return WithEndPoint(ip.ToString(), port);
        }

        public TCPClientEndPointBuilder<TClient, TOptions> WithEndPoint(string ip, int port)
        {
            options.IpAddress = ip;
            options.Port = port;

            return this;
        }

        public void AddReceiveHandle(ReceivePacketDebugInfo<TCPClient<TClient>> handle)
        {
            OnReceiveHandles += handle;
        }

        public void AddSendHandle(SendPacketDebugInfo<TCPClient<TClient>> handle)
        {
            OnSendHandles += handle;
        }

        public void AddBaseReceiveHandle(ReceivePacketDebugInfo<IClient> handle)
        {
            OnReceiveHandles += (client, pid, len) => handle(client, pid, len);
        }

        public void AddBaseSendHandle(SendPacketDebugInfo<IClient> handle)
        {
            OnSendHandles += (client, pid, len,stack) => handle(client, pid, len, stack);
        }

        public TCPNetworkClient<TClient, TOptions> Build()
        {
            var result = new TCPNetworkClient<TClient, TOptions>(options);

            result.OnReceivePacket += OnReceiveHandles;

            result.OnSendPacket += OnSendHandles;

            return result;
        }
    }
}
