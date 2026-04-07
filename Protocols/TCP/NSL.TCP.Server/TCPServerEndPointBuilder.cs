using System;
using System.Net;
using NSL.TCP.Server;
using NSL.SocketServer.Utils;
using NSL.SocketServer;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.EndPointBuilder;

namespace NSL.BuilderExtensions.TCPServer
{
    public class TCPServerEndPointBuilder
    {
        private TCPServerEndPointBuilder() { }

        public static TCPServerEndPointBuilder Create()
        {
            return new TCPServerEndPointBuilder();
        }

        public TCPServerEndPointBuilder<TClient> WithClientProcessor<TClient>()
            where TClient : BaseNetworkConnection, new()
        {
            return TCPServerEndPointBuilder<TClient>.Create();
        }
    }

    public class TCPServerEndPointBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        private TCPServerEndPointBuilder() { }

        public static TCPServerEndPointBuilder<TClient> Create()
        {
            return new TCPServerEndPointBuilder<TClient>();
        }

        public TCPServerEndPointBuilder<TClient, ServerOptions<TClient>> WithOptions()
            => WithOptions<ServerOptions<TClient>>();

        public TCPServerEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : CoreOptions, new()
        {
            return TCPServerEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class TCPServerEndPointBuilder<TClient, TOptions> : IOptionableEndPointBuilder, IHandleIOBuilder<TClient>
        where TClient : BaseNetworkConnection, new()
        where TOptions : CoreOptions, new()
    {
        TOptions options = new TOptions();

        public TOptions GetOptions() => options;

        public CoreOptions GetCoreOptions() => options;

        private TCPServerEndPointBuilder() { }

        public static TCPServerEndPointBuilder<TClient, TOptions> Create()
        {
            return new TCPServerEndPointBuilder<TClient, TOptions>();
        }

        public TCPServerEndPointBuilder<TClient, TOptions> WithCode(Action<TCPServerEndPointBuilder<TClient, TOptions>> code)
        {
            code(this);
            return this;
        }

        public TCPServerEndPointBuilder<TClient, TOptions> WithBindingPoint(IPEndPoint endpoint)
        {
            return WithBindingPoint(endpoint.Address, endpoint.Port);
        }

        public TCPServerEndPointBuilder<TClient, TOptions> WithBindingPoint(IPAddress ip, int port)
        {
            return WithBindingPoint(ip.ToString(), port);
        }

        public TCPServerEndPointBuilder<TClient, TOptions> WithBindingPoint(int port)
            => WithBindingPoint(System.Net.IPAddress.Any.ToString(), port);

        public TCPServerEndPointBuilder<TClient, TOptions> WithBindingPoint(string ip, int port)
        {
            options.WithBindingEndPoint(ip, port);

            return this;
        }

        public TCPServerEndPointBuilder<TClient, TOptions> WithBacklog(int maxWaitConnectionCount)
        {
            var ep = options.GetBindingEndPoint();
            ep.Backlog = maxWaitConnectionCount;
            options.ObjectBag.SetBindingEndPoint(ep);

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

        public TCPServerListener<TClient> Build(bool legacyThread = false)
            => new TCPServerListener<TClient>(options, legacyThread);
    }
}
