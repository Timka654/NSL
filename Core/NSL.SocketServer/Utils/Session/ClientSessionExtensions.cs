using NSL.SocketCore.Utils;
using NSL.SocketCore.Network.Session;
using NSL.SocketServer;
using NSL.SocketServer.Utils;

namespace NSL.SocketServer.Utils.Session
{
    public static class ClientSessionExtensions
    {
        public static NSLSessionServerOptions GetSessionOptions<TClient>(this TClient client)
            where TClient : BaseNetworkConnection
        {
            var so = client.Options as ServerOptions<TClient>;
            return so.ObjectBag.Get<NSLSessionServerOptions>(NSLSessionServerOptions.ObjectBagKey, true);
        }

        public static NSLServerSessionInfo<TClient> GetSessionInfo<TClient>(this TClient client)
            where TClient : BaseNetworkConnection
        {
            var options = client.GetSessionOptions();
            return GetSessionInfo(client, options.ClientSessionBagKey);
        }

        public static NSLServerSessionInfo<TClient> GetSessionInfo<TClient>(this TClient client, string clientBagKey = NSLSessionServerOptions.DefaultSessionBagKey)
            where TClient : BaseNetworkConnection
        {
            client.ThrowIfObjectBagNull();
            return client.ObjectBag.Get<NSLServerSessionInfo<TClient>>(clientBagKey);
        }
    }
}
