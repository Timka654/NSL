using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore;
using static NSL.SocketCore.Extensions.Buffer.RequestExtensions;
using System;
using NSL.EndPointBuilder;
using System.Net.Sockets;
using System.Reflection;

namespace NSL.BuilderExtensions.Buffer
{
    public static class Extensions
    {
        public static void AddRequestPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, RequestPacketHandle<TClient> packet) where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddRequestPacketHandle(packetId, packet);

        public static void AddRequestPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, RequestPacketHandle2<TClient> packet, ushort responsePacketId = 1) where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddRequestPacketHandle(packetId, packet, responsePacketId);

        public static void AddAsyncRequestPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, RequestPacketAsyncHandle<TClient> packet) where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddAsyncRequestPacketHandle(packetId, packet);

        public static void AddAsyncRequestPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, RequestPacketAsyncHandle2<TClient> packet, ushort responsePacketId = 1) where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddAsyncRequestPacketHandle(packetId, packet, responsePacketId);

        /// <summary>
        /// Create <see cref="RequestProcessor"/> with <paramref name="objectKey"/> in Client.ObjectBag and register handle for execute wait receive packet in buffer
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="builder"></param>
        /// <param name="packetId"></param>
        /// <param name="objectKey"></param>
        public static void ConfigureRequestProcessor<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum responsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : INetworkClient, new()
            where TEnum : struct, IConvertible
            => builder.GetCoreOptions().ConfigureRequestProcessor(responsePacketId, objectKey);

        /// <summary>
        /// Create <see cref="RequestProcessor"/> with <paramref name="objectKey"/> in Client.ObjectBag and register handle for execute wait receive packet in buffer
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="builder"></param>
        /// <param name="packetId"></param>
        /// <param name="objectKey"></param>
        public static void ConfigureRequestProcessor<TClient>(this IOptionableEndPointBuilder<TClient> builder, ushort responsePacketId = RequestProcessor.DefaultResponsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : INetworkClient, new()
            => builder.GetCoreOptions().ConfigureRequestProcessor(responsePacketId, objectKey);
    }
}
