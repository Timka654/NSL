using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore;
using NSL.SocketCore.Extensions.Packet;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSL.BuilderExtensions.SocketCore
{
    public static class Extensions
    {
        public static void AddPacket<TClient>(this IOptionableEndPointBuilder<TClient> builder, ushort packetId, IPacket<TClient> packet)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().AddPacket(packetId, packet);
        }

        public static void AddPacket<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, IPacket<TClient> packet)
            where TEnum : struct, IConvertible
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().AddPacket(packetId.ToUInt16(null), packet);
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TPacket">PacketAttribute impl</typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static void LoadPackets<TClient, TPacket>(this IOptionableEndPointBuilder<TClient> builder, Assembly assembly)
            where TPacket : PacketAttribute
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().LoadPackets(assembly, typeof(TPacket), t => (IPacket<TClient>)Activator.CreateInstance(t));
        }

        /// <summary>
        /// with caling assembly
        /// </summary>
        /// <typeparam name="TPacket">PacketAttribute impl</typeparam>
        /// <returns></returns>
        public static void LoadPackets<TClient, TPacket>(this IOptionableEndPointBuilder<TClient> builder)
            where TPacket : PacketAttribute
            where TClient : INetworkClient, new()
        {
            LoadPackets<TClient, TPacket>(builder, Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TPacket">PacketAttribute impl</typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static void LoadPackets<TClient>(this IOptionableEndPointBuilder<TClient> builder, Assembly assembly, Type packetAttributeSelectType)
            where TClient : INetworkClient, new()
        {
            if (packetAttributeSelectType.IsAssignableFrom(typeof(PacketAttribute)))
                throw new ArgumentOutOfRangeException($"paameter {nameof(packetAttributeSelectType)} must be assgnable from {nameof(PacketAttribute)}");


            builder.GetCoreOptions().LoadPackets(assembly, packetAttributeSelectType, t => (IPacket<TClient>)Activator.CreateInstance(t));
        }

        /// <summary>
        /// with caling assembly
        /// </summary>
        /// <typeparam name="TPacket">PacketAttribute impl</typeparam>
        /// <returns></returns>
        public static void LoadPackets<TClient>(this IOptionableEndPointBuilder<TClient> builder, Type packetAttributeSelectType)
            where TClient : INetworkClient, new()
        {
            LoadPackets(builder, Assembly.GetCallingAssembly(), packetAttributeSelectType);
        }

        public static void WithInputCipher<TClient>(this IOptionableEndPointBuilder<TClient> builder, IPacketCipher cipher)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().InputCipher = cipher;
        }

        public static void WithOutputCipher<TClient>(this IOptionableEndPointBuilder<TClient> builder, IPacketCipher cipher)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OutputCipher = cipher;
        }

        public static void WithAddressFamily<TClient>(this IOptionableEndPointBuilder<TClient> builder, AddressFamily family)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().AddressFamily = family;
        }

        public static void WithProtocolType<TClient>(this IOptionableEndPointBuilder<TClient> builder, ProtocolType type)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().ProtocolType = type;
        }

        public static void WithBufferSize<TClient>(this IOptionableEndPointBuilder<TClient> builder, int size)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().ReceiveBufferSize = size;
        }
        public static void AddConnectHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.ClientConnect handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnClientConnectEvent += handle;
        }

        public static void AddDisconnectHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.ClientDisconnect handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnClientDisconnectEvent += handle;
        }

        public static void AddExceptionHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.ExceptionHandle handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnExceptionEvent += handle;
        }

        public static void AddConnectAsyncHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.ClientConnectAsync handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnClientConnectAsyncEvent += handle;
        }

        public static void AddDisconnectAsyncHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.ClientDisconnectAsync handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnClientDisconnectAsyncEvent += handle;
        }

        public static void SetLogger<TClient>(this IOptionableEndPointBuilder<TClient> builder, IBasicLogger logger)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().HelperLogger = logger;
        }

    }
}
