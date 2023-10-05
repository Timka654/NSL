using NSL.EndPointBuilder;
using NSL.SocketCore;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.UDP;
using NSL.UDP.Client;
using System;

namespace NSL.BuilderExtensions.UDPClient
{
    public class UDPClientEndPointBuilder
    {
        private UDPClientEndPointBuilder() { }

        public static UDPClientEndPointBuilder Create()
        {
            return new UDPClientEndPointBuilder();
        }

        public UDPClientEndPointBuilder<TClient> WithClientProcessor<TClient>()
            where TClient : IServerNetworkClient, new()
        {
            return UDPClientEndPointBuilder<TClient>.Create();
        }
    }

    public class UDPClientEndPointBuilder<TClient>
        where TClient : IServerNetworkClient, new()
    {
        private UDPClientEndPointBuilder() { }

        public static UDPClientEndPointBuilder<TClient> Create()
        {
            return new UDPClientEndPointBuilder<TClient>();
        }

        public UDPClientEndPointBuilder<TClient, UDPClientOptions<TClient>> WithOptions()
            => WithOptions<UDPClientOptions<TClient>>();

        public UDPClientEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : UDPClientOptions<TClient>, new()
        {
            return UDPClientEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class UDPClientEndPointBuilder<TClient, TOptions> : IOptionableEndPointServerBuilder<TClient>, IHandleIOBuilder<TClient>
        where TClient : IServerNetworkClient, new()
        where TOptions : UDPClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        public ServerOptions<TClient> GetOptions() => options;

        public CoreOptions<TClient> GetCoreOptions() => options;

        private UDPClientEndPointBuilder() { }

        public static UDPClientEndPointBuilder<TClient, TOptions> Create()
        {
            return new UDPClientEndPointBuilder<TClient, TOptions>();
        }

        public UDPClientEndPointBuilder<TClient, TOptions> UseBindingPoint(string ipAddress, int port)
        {
            options.BindingIP = ipAddress;
            options.BindingPort = port;

            return this;
        }

        public UDPClientEndPointBuilder<TClient, TOptions> UseEndPoint(string ipAddress, int port)
        {
            options.IpAddress = ipAddress;
            options.Port = port;

            return this;
        }

        public UDPClientEndPointBuilder<TClient, TOptions> WithCode(Action<UDPClientEndPointBuilder<TClient, TOptions>> code)
        {
            code(this);
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

        public UDPNetworkClient<TClient> Build()
            => new UDPNetworkClient<TClient>(options);
    }
}
