using NSL.SocketCore;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace NSL.UDP.Client
{
    public class UDPServer<TClient> : UDPListener<TClient>, INetworkListener
        where TClient : IServerNetworkClient, new()
    {
        public UDPServer(UDPOptions<TClient> options) : base(options)
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
            if (!state)
                return;

            RunReceiveAsync();

            clients.GetOrAdd(e.RemoteEndPoint as IPEndPoint, ipep => new UDPClient<TClient>(ipep, listener, options)).Receive(e);
        }

        public int GetListenerPort() => options.BindingPort;

        public CoreOptions GetOptions() => options;

        public ServerOptions<TClient> GetServerOptions() => options;
    }
}
