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
using System.Threading.Tasks;
using static NSL.SocketCore.Utils.Request.RequestExtensions;
using NSL.SocketCore.Utils.Logger.Enums;
using NSL.SocketCore.Network;

namespace NSL.BuilderExtensions.SocketCore
{
    public static class Extensions
    {
        public static void AddPacketHandle(this IOptionableEndPointBuilder builder, ushort packetId, Action<BaseNetworkConnection, InputPacketBuffer> handle)
        {
            builder.GetCoreOptions().AddHandle(packetId, handle);
        }

        public static void AddPacketHandle<TEnum>(this IOptionableEndPointBuilder builder, TEnum packetId, Action<BaseNetworkConnection, InputPacketBuffer> packet)
            where TEnum : struct, IConvertible
        {
            AddPacketHandle(builder, packetId.ToUInt16(null), packet);
        }

        public static void AddAsyncPacketHandle(this IOptionableEndPointBuilder builder, ushort packetId, Func<BaseNetworkConnection, InputPacketBuffer, Task> handle)
        {
            builder.GetCoreOptions().AddAsyncHandle(packetId, handle);
        }

        public static void AddAsyncPacketHandle<TEnum>(this IOptionableEndPointBuilder builder, TEnum packetId, Func<BaseNetworkConnection, InputPacketBuffer, Task> packet)
            where TEnum : struct, IConvertible
        {
            AddAsyncPacketHandle(builder, packetId.ToUInt16(null), packet);
        }
    }
}
