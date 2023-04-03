using NSL.BuilderExtensions.UDP;
using NSL.EndPointBuilder;
using NSL.SocketCore;
using System;
using System.Net;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.UDP.Client;
using NSL.UDP;

namespace NSL.BuilderExtensions.UDPServer
{
    public class UDPServerEndPointBuilder : UDPEndPointBuilder
    {
        private UDPServerEndPointBuilder() { }

        public static UDPServerEndPointBuilder Create()
        {
            return new UDPServerEndPointBuilder();
        }

        public UDPServerEndPointBuilder<TClient> WithClientProcessor<TClient>()
            where TClient : IServerNetworkClient, new()
        {
            return UDPServerEndPointBuilder<TClient>.Create();
        }
    }

    public class UDPServerEndPointBuilder<TClient> : UDPEndPointBuilder
        where TClient : IServerNetworkClient, new()
    {
        private UDPServerEndPointBuilder() { }

        public static UDPServerEndPointBuilder<TClient> Create()
        {
            return new UDPServerEndPointBuilder<TClient>();
        }

        public UDPServerEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : UDPClientOptions<TClient>, new()
        {
            return UDPServerEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class UDPServerEndPointBuilder<TClient, TOptions> : UDPEndPointBuilder, IOptionableEndPointServerBuilder<TClient>, IHandleIOBuilder<UDPClient<TClient>>
        where TClient : IServerNetworkClient, new()
        where TOptions : UDPClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        event ReceivePacketDebugInfo<UDPClient<TClient>> OnReceiveHandles;

        event SendPacketDebugInfo<UDPClient<TClient>> OnSendHandles;

        public ServerOptions<TClient> GetOptions() => options;

        public CoreOptions<TClient> GetCoreOptions() => options;

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

        public void AddReceiveHandle(ReceivePacketDebugInfo<UDPClient<TClient>> handle)
        {
            OnReceiveHandles += handle;
        }

        public void AddSendHandle(SendPacketDebugInfo<UDPClient<TClient>> handle)
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

        public UDPServer<TClient> Build()
        {
            var result = new UDPServer<TClient>(options);

            result.OnReceivePacket += OnReceiveHandles;

            result.OnSendPacket += OnSendHandles;

            return result;
        }
    }
}
