using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NSL.UDP.Client
{
    public class UDPServer<TClient> : UDPListener<TClient, UDPClientOptions<TClient>>, INetworkListener
        where TClient : BaseNetworkConnection, new()
    {
        public event ReceivePacketDebugInfo<UDPClient<TClient>> OnReceivePacket;

        public event SendPacketDebugInfo<UDPClient<TClient>> OnSendPacket;

        public UDPServer(UDPClientOptions<TClient> options) : base(options)
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

        private ConcurrentDictionary<IPEndPoint, Lazy<UDPClient<TClient>>> clients = new ConcurrentDictionary<IPEndPoint, Lazy<UDPClient<TClient>>>();

        protected override void Options_OnClientDisconnectEvent(TClient client)
        {
            if (client?.Network != null)
                clients.TryRemove((client.Network as UDPClient<TClient>).GetRemotePoint(), out var _);
        }

        protected override void Args_Completed(Span<byte> buffer, SocketReceiveFromResult e, CancellationToken token)
        {
            if (!state || ListenerCTS.IsCancellationRequested)
                return;

            RunReceiveIntern(token);

            GetClient(e.RemoteEndPoint as IPEndPoint)
                .Receive(buffer);
        }

        private UDPClient<TClient> GetClient(IPEndPoint endPoint)
        {
            var lazy = clients.GetOrAdd(endPoint, ipep =>
                new Lazy<UDPClient<TClient>>(() =>
                {
                    var client = new UDPClient<TClient>(ipep, listener, options);
                    client.OnReceivePacket += OnReceivePacket;
                    client.OnSendPacket += OnSendPacket;
                    return client;
                }));

            try
            {
                return lazy.Value;
            }
            catch
            {
                clients.TryRemove(endPoint, out _);
                throw;
            }
        }

        public UDPClient<TClient> CreateClientConnection(IPEndPoint endPoint)
            => GetClient(endPoint);

        public int GetListenerPort() => options.BindingPort;

        public CoreOptions GetOptions() => options;
    }
}
