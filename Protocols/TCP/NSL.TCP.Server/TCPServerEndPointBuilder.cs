using System;
using System.Net;
using NSL.TCP.Server;
using NSL.SocketServer.Utils;
using NSL.SocketServer;
using NSL.SocketCore;
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
            where TClient : IServerNetworkClient, new()
        {
            return TCPServerEndPointBuilder<TClient>.Create();
        }
    }

    public class TCPServerEndPointBuilder<TClient>
        where TClient : IServerNetworkClient, new()
    {
        private TCPServerEndPointBuilder() { }

        public static TCPServerEndPointBuilder<TClient> Create()
        {
            return new TCPServerEndPointBuilder<TClient>();
        }

        public TCPServerEndPointBuilder<TClient, ServerOptions<TClient>> WithOptions()
            => WithOptions<ServerOptions<TClient>>();

        public TCPServerEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : ServerOptions<TClient>, new()
        {
            return TCPServerEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class TCPServerEndPointBuilder<TClient, TOptions> : IOptionableEndPointServerBuilder<TClient>, IHandleIOBuilder<TClient>
        where TClient : IServerNetworkClient, new()
        where TOptions : ServerOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        public ServerOptions<TClient> GetOptions() => options;

        public CoreOptions<TClient> GetCoreOptions() => options;

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
            options.IpAddress = ip;
            options.Port = port;

            return this;
        }

        public TCPServerEndPointBuilder<TClient, TOptions> WithBacklog(int maxWaitConnectionCount)
        {
            options.Backlog = maxWaitConnectionCount;

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

        public TCPServerListener<TClient> Build(bool legacyThread = false)
            => new TCPServerListener<TClient>(options, legacyThread);
    }
}
