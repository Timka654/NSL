using NSL.Extensions.Version.Server.Packets;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;

namespace NSL.Extensions.Version.Server
{
    public static class ServerExtensions
    {
        public static ServerOptions<TClient> AddNSLVersion<TClient>(this ServerOptions<TClient> options, Action<NSLServerVersionInfo> configure = null)
            where TClient : IServerNetworkClient
        {
            var sOptions = new NSLServerVersionInfo();

            if (configure != null)
                configure(sOptions);

            return AddNSLVersion(options, sOptions);
        }

        public static ServerOptions<TClient> AddNSLVersion<TClient>(this ServerOptions<TClient> options, NSLServerVersionInfo versionInfo)
            where TClient : IServerNetworkClient
        {
            options.ObjectBag.Set(NSLServerVersionInfo.ObjectBagKey, versionInfo);

            options.AddPacket(NSLVersionPacket<TClient>.PacketId, new NSLVersionPacket<TClient>());

            return options;
        }
    }
}
