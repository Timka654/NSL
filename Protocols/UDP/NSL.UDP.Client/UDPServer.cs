using NSL.SocketCore;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NSL.UDP.Client
{
    public class IPEPEQComparer : IEqualityComparer<IPEndPoint>
    {
        public bool Equals(IPEndPoint x, IPEndPoint y)
        {
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] IPEndPoint obj)
        {
            return obj.GetHashCode();
        }
    }

    public class UDPServer<TClient> : UDPListener<TClient, UDPServerOptions<TClient>>, INetworkListener
        where TClient : IServerNetworkClient, new()
    {
        public event ReceivePacketDebugInfo<UDPClient<TClient>> OnReceivePacket;

        public event SendPacketDebugInfo<UDPClient<TClient>> OnSendPacket;

        public UDPServer(UDPServerOptions<TClient> options) : base(options)
        {
        }

        public void Start()
        {
            base.StartReceive();
        }

        public void Stop()
        {
            StopReceive();
        }

        private ConcurrentDictionary<IPEndPoint, Lazy<UDPClient<TClient>>> clients = new ConcurrentDictionary<IPEndPoint, Lazy<UDPClient<TClient>>>(new IPEPEQComparer());

        protected override void Options_OnClientDisconnectEvent(TClient client)
        {
            if (client?.Network != null)
                clients.TryRemove((client.Network as UDPClient<TClient>).GetRemotePoint(), out var _);
        }

        protected override void Args_Completed(Span<byte> buffer, SocketReceiveFromResult e)
        {
            if (!state || ListenerCTS.IsCancellationRequested)
                return;

            RunReceiveAsync();

            var c = clients.GetOrAdd(e.RemoteEndPoint as IPEndPoint, ipep =>
            {
                return new Lazy<UDPClient<TClient>>(() =>
                {
                    var client = new UDPClient<TClient>(ipep, listener, options);
                    client.OnReceivePacket += OnReceivePacket;
                    client.OnSendPacket += OnSendPacket;
                    return client;
                });
            });

            c.Value.Receive(buffer[..e.ReceivedBytes]);
        }

        public int GetListenerPort() => options.BindingPort;

        public CoreOptions GetOptions() => options;

        public ServerOptions<TClient> GetServerOptions() => options;
    }
}
