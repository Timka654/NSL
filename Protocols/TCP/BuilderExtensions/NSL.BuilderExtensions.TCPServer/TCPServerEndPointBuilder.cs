using NSL.BuilderExtensions.TCP;
using NSL.EndPointBuilder;
using NSL.SocketClient.Utils;
using NSL.SocketClient;
using NSL.SocketCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NSL.TCP.Server;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.SocketCore.Utils.Logger;

namespace NSL.BuilderExtensions.TCPServer
{
    public class TCPServerEndPointBuilder : TCPEndPointBuilder
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

    public class TCPServerEndPointBuilder<TClient> : TCPEndPointBuilder
        where TClient : IServerNetworkClient, new()
    {
        private TCPServerEndPointBuilder() { }

        public static TCPServerEndPointBuilder<TClient> Create()
        {
            return new TCPServerEndPointBuilder<TClient>();
        }

        public TCPServerEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : ServerOptions<TClient>, new()
        {
            return TCPServerEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class TCPServerEndPointBuilder<TClient, TOptions> : TCPEndPointBuilder, IOptionableEndPointServerBuilder<TClient>, IHandleIOBuilder<TCPServerClient<TClient>>
        where TClient : IServerNetworkClient, new()
        where TOptions : ServerOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        event ReceivePacketDebugInfo<TCPServerClient<TClient>> OnReceiveHandles;

        event SendPacketDebugInfo<TCPServerClient<TClient>> OnSendHandles;

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

        public void AddReceiveHandle(ReceivePacketDebugInfo<TCPServerClient<TClient>> handle)
        {
            OnReceiveHandles += handle;
        }

        public void AddSendHandle(SendPacketDebugInfo<TCPServerClient<TClient>> handle)
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

        public TCPServerListener<TClient> Build()
        {
            var result = new TCPServerListener<TClient>(options);

            result.OnReceivePacket += OnReceiveHandles;

            result.OnSendPacket += OnSendHandles;

            return result;
        }
    }
}
