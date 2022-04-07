using NSL.SocketClient.Node.Packets;
using SocketServer;
using SocketServer.Utils;
using System.Collections.Concurrent;

namespace NSL.SocketClient.Node
{
    public class NodeHub<TClient>
        where TClient : INodeNetworkClient
    {
        private INetworkListener<TClient> listener;

        public string ConnectionToken { get; private set; }

        private ConcurrentDictionary<string, TClient> players = new();

        ServerOptions<TClient> Options => listener.GetServerOptions();

        public NodeHub(INetworkListener<TClient> listener, string connectionToken)
        {
            this.listener = listener;

            ConnectionToken = connectionToken;

            Options.AddPacket(1, new SignInPacket<TClient>(this));
            //Options.AddHandle(2, SignIn);
        }

        internal void AddPlayer(TClient client)
        {
            if (!players.TryAdd(client.PlayerId, client))
                client.Dispose();
        }
    }
}
