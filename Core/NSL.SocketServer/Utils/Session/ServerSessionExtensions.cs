using NSL.SocketCore.Utils;
using NSL.SocketCore.Network.Session;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;

namespace NSL.SocketServer.Utils.Session
{
    public static class ServerSessionExtensions
    {
        public static NSLSessionManager<TClient> AddNSLSessions<TClient>(this ServerOptions<TClient> options, Action<NSLSessionServerOptions<TClient>> configure = null)
            where TClient : IServerNetworkClient
        {
            var sOptions = new NSLSessionServerOptions<TClient>();
            configure?.Invoke(sOptions);

            var manager = new NSLSessionManager<TClient>(sOptions, options);

            options.ObjectBag.Set(NSLSessionServerOptions.ObjectBagKey, sOptions);
            options.ObjectBag.Set(NSLSessionManager<TClient>.ObjectBagKey, manager);
            manager.RegisterServer(options);

            return manager;
        }
    }
}
