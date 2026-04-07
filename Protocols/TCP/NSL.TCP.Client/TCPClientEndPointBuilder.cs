using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.TCP.Client;
using System;
using System.Net;

namespace NSL.BuilderExtensions.TCPClient
{
    public class TCPClientEndPointBuilder
    {
        private TCPClientEndPointBuilder() { }

        public static TCPClientEndPointBuilder Create()
        {
            return new TCPClientEndPointBuilder();
        }

        public TCPClientEndPointBuilder<TClient> WithClientProcessor<TClient>()
            where TClient : BaseNetworkConnection, new()
        {
            return TCPClientEndPointBuilder<TClient>.Create();
        }
    }

    public class TCPClientEndPointBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        private TCPClientEndPointBuilder() { }

        public static TCPClientEndPointBuilder<TClient> Create()
        {
            return new TCPClientEndPointBuilder<TClient>();
        }

        public TCPClientEndPointBuilder<TClient, ClientOptions<TClient>> WithOptions()
            => WithOptions<ClientOptions<TClient>>();

        public TCPClientEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : CoreOptions, new()
        {
            return TCPClientEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class TCPClientEndPointBuilder<TClient, TOptions> : IOptionableEndPointBuilder, IHandleIOBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
        where TOptions : CoreOptions, new()
    {
        TOptions options = new TOptions();

        public TOptions GetOptions() => options;

        public CoreOptions GetCoreOptions() => options;

        private TCPClientEndPointBuilder() { }

        public static TCPClientEndPointBuilder<TClient, TOptions> Create()
        {
            return new TCPClientEndPointBuilder<TClient, TOptions>();
        }

        public TCPClientEndPointBuilder<TClient, TOptions> WithCode(Action<TCPClientEndPointBuilder<TClient, TOptions>> code)
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
            options.WithRemoteEndPoint(ip, port);

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

        public TCPNetworkClient<TClient> Build(bool legacyThread = false)
            => new TCPNetworkClient<TClient>(options, legacyThread);
    }
}
