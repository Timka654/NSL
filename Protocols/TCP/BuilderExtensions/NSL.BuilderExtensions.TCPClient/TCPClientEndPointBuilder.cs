using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore;
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
            where TClient : BaseSocketNetworkClient, new()
        {
            return TCPClientEndPointBuilder<TClient>.Create();
        }
    }

    public class TCPClientEndPointBuilder<TClient>
        where TClient : BaseSocketNetworkClient, new()
    {
        private TCPClientEndPointBuilder() { }

        public static TCPClientEndPointBuilder<TClient> Create()
        {
            return new TCPClientEndPointBuilder<TClient>();
        }

        public TCPClientEndPointBuilder<TClient, ClientOptions<TClient>> WithOptions()
            => WithOptions<ClientOptions<TClient>>();

        public TCPClientEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : ClientOptions<TClient>, new()
        {
            return TCPClientEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class TCPClientEndPointBuilder<TClient, TOptions> : IOptionableEndPointClientBuilder<TClient>, IHandleIOBuilder<TClient>
        where TClient : BaseSocketNetworkClient, new()
        where TOptions : ClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        public ClientOptions<TClient> GetOptions() => options;

        public CoreOptions<TClient> GetCoreOptions() => options;

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
            options.IpAddress = ip;
            options.Port = port;

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

        public TCPNetworkClient<TClient, TOptions> Build(bool legacyThread = false)
            => new TCPNetworkClient<TClient, TOptions>(options, legacyThread);
    }
}
