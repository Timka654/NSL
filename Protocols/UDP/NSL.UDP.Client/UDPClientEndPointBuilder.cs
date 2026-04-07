using NSL.EndPointBuilder;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
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
            where TClient : BaseNetworkConnection, new()
        {
            return UDPClientEndPointBuilder<TClient>.Create();
        }
    }

    public class UDPClientEndPointBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
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

    public class UDPClientEndPointBuilder<TClient, TOptions> : IOptionableEndPointBuilder, IHandleIOBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
        where TOptions : UDPClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        public CoreOptions GetCoreOptions() => options;

        private UDPClientEndPointBuilder() { }

        public static UDPClientEndPointBuilder<TClient, TOptions> Create()
        {
            return new UDPClientEndPointBuilder<TClient, TOptions>();
        }

        public UDPClientEndPointBuilder<TClient, TOptions> UseBindingPoint(string ipAddress, int port)
        {
            options.WithBindingEndPoint(ipAddress, port);

            return this;
        }

        public UDPClientEndPointBuilder<TClient, TOptions> UseEndPoint(string ipAddress, int port)
        {
            options.WithRemoteEndPoint(ipAddress, port);

            return this;
        }

        public UDPClientEndPointBuilder<TClient, TOptions> WithCode(Action<UDPClientEndPointBuilder<TClient, TOptions>> code)
        {
            code(this);
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

        public UDPNetworkClient<TClient> Build()
            => new UDPNetworkClient<TClient>(options);
    }
}
