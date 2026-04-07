using Microsoft.Extensions.DependencyInjection;
using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Request;
using NSL.SocketCore.Utils.Exceptions;
using NSL.SocketCore.Utils.Logger;

using NSL.SocketCore.Utils.Packet;
using System;
using System.Net.Sockets;
using System.Reflection;
using static NSL.SocketCore.Utils.Request.RequestExtensions;
using NSL.SocketCore.Utils.Logger.Enums;
using NSL.SocketCore.Network;

namespace NSL.BuilderExtensions.SocketCore
{
    public static class Extensions
    {
        public static void AddPacketHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, ushort packetId, ClientOptions<TClient>.PacketHandle handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().AddHandle(packetId, handle);
        }

        public static void AddPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, ClientOptions<TClient>.PacketHandle packet)
            where TEnum : struct, IConvertible
            where TClient : INetworkClient, new()
        {
            AddPacketHandle(builder, packetId.ToUInt16(null), packet);
        }

        public static void AddAsyncPacketHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, ushort packetId, ClientOptions<TClient>.AsyncPacketHandle handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().AddAsyncHandle(packetId, handle);
        }

        public static void AddAsyncPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, ClientOptions<TClient>.AsyncPacketHandle packet)
            where TEnum : struct, IConvertible
            where TClient : INetworkClient, new()
        {
            AddAsyncPacketHandle(builder, packetId.ToUInt16(null), packet);
        }
    }
}
