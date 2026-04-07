using NSL.SocketCore.Utils;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Reflection;

namespace NSL.SocketServer.Utils.Packet
{
    public static class PacketHelper
    {
        public static int LoadPackets<T>(this ServerOptions<T> serverOptions, Assembly assembly, Type selectAttributeType)
            where T : BaseNetworkConnection
            => NSL.SocketCore.Utils.Packet.PacketHelper.LoadPackets(
                serverOptions, assembly, selectAttributeType,
                type => Activator.CreateInstance(type) as IPacket<T>);

        public static int LoadPackets<T>(this ServerOptions<T> serverOptions, Type selectAttributeType)
            where T : BaseNetworkConnection
            => LoadPackets(serverOptions, Assembly.GetCallingAssembly(), selectAttributeType);
    }
}
