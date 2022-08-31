﻿using NSL.BuilderExtensions.UDP;
using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore;
using NSL.SocketServer.Utils;
using NSL.UDP.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace NSL.BuilderExtensions.UDPClient
{
    public class UDPClientEndPointBuilder : UDPEndPointBuilder
    {
        private UDPClientEndPointBuilder() { }

        public static UDPClientEndPointBuilder Create()
        {
            return new UDPClientEndPointBuilder();
        }

        public UDPClientEndPointBuilder<TClient> WithClientProcessor<TClient>()
            where TClient : BaseSocketNetworkClient, new()
        {
            return UDPClientEndPointBuilder<TClient>.Create();
        }
    }

    public class UDPClientEndPointBuilder<TClient> : UDPEndPointBuilder
        where TClient : BaseSocketNetworkClient, new()
    {
        private UDPClientEndPointBuilder() { }

        public static UDPClientEndPointBuilder<TClient> Create()
        {
            return new UDPClientEndPointBuilder<TClient>();
        }

        public UDPClientEndPointBuilder<TClient, TOptions> WithOptions<TOptions>()
            where TOptions : UDPClientOptions<TClient>, new()
        {
            return UDPClientEndPointBuilder<TClient, TOptions>.Create();
        }
    }

    public class UDPClientEndPointBuilder<TClient, TOptions> : UDPEndPointBuilder, IOptionableEndPointClientBuilder<TClient>, IHandleIOBuilder<UDPClient<TClient>>
        where TClient : BaseSocketNetworkClient, new()
        where TOptions : UDPClientOptions<TClient>, new()
    {
        TOptions options = new TOptions();

        event ReceivePacketDebugInfo<UDPClient<TClient>> OnReceiveHandles;

        event SendPacketDebugInfo<UDPClient<TClient>> OnSendHandles;

        //public ClientOptions<TClient> GetOptions() => options;

        public ClientOptions<TClient> GetOptions() => options;

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

        public new UDPClientEndPointBuilder<TClient, TOptions> WithCode(Action<UDPClientEndPointBuilder<TClient, TOptions>> code)
        {
            code(this);
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

        public UDPNetworkClient<TClient> Build()
        {
            var result = new UDPNetworkClient<TClient>(options);

            result.OnReceivePacket += OnReceiveHandles;

            result.OnSendPacket += OnSendHandles;

            return result;
        }

        public ReceivePacketDebugInfo<UDPClient<TClient>> GetReceiveHandles()
        {
            return OnReceiveHandles;
        }

        public SendPacketDebugInfo<UDPClient<TClient>> GetSendHandles()
        {
            return OnSendHandles;
        }
    }
}