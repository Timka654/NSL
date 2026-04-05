using NSL.SocketCore.Utils.Version;
using NSL.SocketServer.Utils;
using System;

namespace NSL.SocketServer.Utils.Version
{
    public static class ServerVersionExtensions
    {
        public static ServerOptions<TClient> AddNSLVersion<TClient>(this ServerOptions<TClient> options, Action<NSLServerVersionInfo> configure = null)
            where TClient : IServerNetworkClient
        {
            var info = new NSLServerVersionInfo();
            configure?.Invoke(info);
            return options.AddNSLVersion(info);
        }

        public static ServerOptions<TClient> AddNSLVersion<TClient>(this ServerOptions<TClient> options, NSLServerVersionInfo versionInfo)
            where TClient : IServerNetworkClient
        {
            options.ObjectBag.Set(NSLVersionInfo.ObjectBagKey, versionInfo);
            options.AddPacket(NSLVersionPacket<TClient>.PacketId, new NSLVersionPacket<TClient>());
            return options;
        }
    }
}
