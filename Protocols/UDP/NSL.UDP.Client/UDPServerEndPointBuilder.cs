using NSL.EndPointBuilder;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using System;
using System.Net;
using NSL.UDP.Client;
using NSL.UDP;

namespace NSL.BuilderExtensions.UDPServer
{
    public class UDPServerEndPointBuilder
    {
        private UDPServerEndPointBuilder() { }

        public static UDPServerEndPointBuilder Create()
        {
            return new UDPServerEndPointBuilder();
        }

        public UDPServerEndPointBuilder<TClient> WithClientProcessor<TClient>()
            where TClient : BaseNetworkConnection, new()
        {
            return UDPServerEndPointBuilder<TClient>.Create();
        }
    }

    public class UDPServerEndPointBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        private UDPServerEndPointBuilder() { }

        public static UDPServerEndPointBuilder<TClient> Create()
        {
            return new UDPServerEndPointBuilder<TClient>();
        }

        public UDPServerEndPointBuilder<TClient, UDPClientOptions<TClient>> WithOptions()
            => WithOptions<UDPClientOptions<TClient>>();

        public UDPServerEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : UDPClientOptions<TClient>, new()
        {
            return UDPServerEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class UDPServerEndPointBuilder<TClient, TOptions> : IOptionableEndPointBuilder, IHandleIOBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
        where TOptions : UDPClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        public CoreOptions GetCoreOptions() => options;

        private UDPServerEndPointBuilder() { }

        public static UDPServerEndPointBuilder<TClient, TOptions> Create()
        {
            return new UDPServerEndPointBuilder<TClient, TOptions>();
        }

        public UDPServerEndPointBuilder<TClient, TOptions> WithCode(Action<UDPServerEndPointBuilder<TClient, TOptions>> code)
        {
            code(this);
            return this;
        }

        public UDPServerEndPointBuilder<TClient, TOptions> WithBindingPoint(IPEndPoint endpoint)
        {
            return WithBindingPoint(endpoint.Address, endpoint.Port);
        }

        public UDPServerEndPointBuilder<TClient, TOptions> WithBindingPoint(IPAddress ip, int port)
        {
            return WithBindingPoint(ip.ToString(), port);
        }

        public UDPServerEndPointBuilder<TClient, TOptions> WithBindingPoint(string ip, int port)
        {
            options.BindingIP = ip;
            options.BindingPort = port;

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

        public UDPServer<TClient> Build()
            => new UDPServer<TClient>(options);
    }
}
