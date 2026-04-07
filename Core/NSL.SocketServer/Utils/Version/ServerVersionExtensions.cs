using Microsoft.Extensions.DependencyInjection;
using NSL.SocketCore.Network.Version;
using NSL.SocketServer.Utils;
using NSL.SocketServer.Utils.Version.Packets;
using System;

namespace NSL.SocketServer.Utils.Version
{
    public static class ServerVersionExtensions
    {
        public static ServerOptions<TClient> AddNSLVersion<TClient>(this ServerOptions<TClient> options, Action<NSLServerVersionInfo> configure = null)
            where TClient : BaseNetworkConnection
        {
            var info = new NSLServerVersionInfo();
            configure?.Invoke(info);
            return options.AddNSLVersion(info);
        }

        public static ServerOptions<TClient> AddNSLVersion<TClient>(this ServerOptions<TClient> options, NSLServerVersionInfo versionInfo)
            where TClient : BaseNetworkConnection
        {
            options.ObjectBag.Set(NSLVersionInfo.ObjectBagKey, versionInfo);
            options.AddPacket(NSLVersionPacketReceive<TClient>.PacketId, new NSLVersionPacketReceive<TClient>());
            return options;
        }

        public static ServerOptions<TClient> AddNSLVersion<TClient>(this ServerOptions<TClient> options, IServiceCollection services, Action<NSLServerVersionInfo> configure = null)
            where TClient : BaseNetworkConnection
        {
            var info = new NSLServerVersionInfo();
            configure?.Invoke(info);
            services.AddSingleton(info);
            return options.AddNSLVersion(info);
        }

        public static ServerOptions<TClient> AddNSLVersion<TClient>(this ServerOptions<TClient> options, IServiceCollection services, NSLServerVersionInfo versionInfo)
            where TClient : BaseNetworkConnection
        {
            services.AddSingleton(versionInfo);
            return options.AddNSLVersion(versionInfo);
        }
    }
}
