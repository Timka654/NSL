using NSL.SocketCore;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace NSL.UDP.Client
{
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

        private ConcurrentDictionary<IPEndPoint, UDPClient<TClient>> clients = new ConcurrentDictionary<IPEndPoint, UDPClient<TClient>>();

        protected override void Options_OnClientDisconnectEvent(TClient client)
        {
            if (client?.Network != null)
                clients.TryRemove((client.Network as UDPClient<TClient>).GetRemotePoint(), out var _);
        }

        protected override void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (!state || ListenerCTS.IsCancellationRequested)
                return;

            RunReceiveAsync(ListenerCTS.Token);

            var c = clients.GetOrAdd(e.RemoteEndPoint as IPEndPoint, ipep =>
            {
                var client = new UDPClient<TClient>(ipep, listener, options);
                client.OnReceivePacket += OnReceivePacket;
                client.OnSendPacket += OnSendPacket;
                return client;
            });

            c.Receive(e);
        }

        public int GetListenerPort() => options.BindingPort;

        public CoreOptions GetOptions() => options;

        public ServerOptions<TClient> GetServerOptions() => options;
    }
}
