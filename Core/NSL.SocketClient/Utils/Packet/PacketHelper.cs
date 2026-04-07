using NSL.SocketClient;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Packet;
using System;
using System.Reflection;

namespace NSL.SocketClient.Utils.Packet
{
    public static class PacketHelper
    {
        public static int LoadPackets<T>(this ClientOptions<T> clientOptions, Assembly assembly, Type selectAttributeType)
            where T : BaseNetworkConnection
            => NSL.SocketCore.Utils.Packet.PacketHelper.LoadPackets(
                clientOptions, assembly, selectAttributeType,
                type => Activator.CreateInstance(type, clientOptions) as IPacket<T>);

        public static int LoadPackets<T>(this ClientOptions<T> clientOptions, Type selectAttributeType)
            where T : BaseNetworkConnection
            => LoadPackets(clientOptions, Assembly.GetCallingAssembly(), selectAttributeType);
    }
}
