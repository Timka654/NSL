using NSL.Extensions.Session.Server.Packets;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;

namespace NSL.Extensions.Session.Server
{
    public static class ServerExtensions
    {
        public static NSLSessionManager<TClient> AddNSLSessions<TClient>(this ServerOptions<TClient> options, Action<NSLSessionServerOptions<TClient>> configure = null)
            where TClient : IServerNetworkClient
        {
            var sOptions = new NSLSessionServerOptions<TClient>();

            if (configure != null)
                configure(sOptions);

            var manager = new NSLSessionManager<TClient>(sOptions);

            options.ObjectBag.Set(NSLSessionServerOptions.ObjectBagKey, sOptions);
            options.ObjectBag.Set(NSLSessionManager<TClient>.ObjectBagKey, manager);

            manager.RegisterServer(options);

            return manager;
        }
    }
}
