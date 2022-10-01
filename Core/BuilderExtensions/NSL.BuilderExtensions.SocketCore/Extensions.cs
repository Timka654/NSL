using NSL.EndPointBuilder;
using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Extensions.Packet;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Logger;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Net.Sockets;
using System.Reflection;

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


        #region DefaultHandles

        private static Func<ushort, string> defaultGetNamePacketHandle = pid => default;

        #endregion

        public static void AddDefaultEventHandlers<TBuilder, TClient>(this TBuilder builder,
            string prefix = default,
            DefaultEventHandlersEnum handleOptions = DefaultEventHandlersEnum.All,
            Func<ushort, string> getNameSendPacket = default,
            Func<ushort, string> getNameReceivePacket = default)
            where TBuilder : IOptionableEndPointBuilder<TClient>, IHandleIOBuilder
            where TClient : INetworkClient, new()
        {
            if (prefix != null)
                prefix = $"{prefix} ";

            var options = builder
               .GetCoreOptions();

            var logger = builder
               .GetCoreOptions()
               .HelperLogger;

            if (logger == default)
                throw new InvalidOperationException($"{nameof(CoreOptions.HelperLogger)} has not setted in options");

            if (getNameSendPacket == default)
                getNameSendPacket = defaultGetNamePacketHandle;

            if (getNameReceivePacket == default)
                getNameReceivePacket = defaultGetNamePacketHandle;


            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Connect))
                builder.AddConnectHandle(client
                    => logger.Append(LoggerLevel.Info, $"{prefix}Success connected"
                        + (handleOptions.HasFlag(DefaultEventHandlersEnum.DisplayEndPoint) ? $"({client.Network?.GetRemotePoint()})" : default)));

            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Disconnect))
                builder.AddDisconnectHandle(client
                    => logger.Append(LoggerLevel.Info, $"{prefix}Success disconnected"
                        + (handleOptions.HasFlag(DefaultEventHandlersEnum.DisplayEndPoint) ? $"({client.Network?.GetRemotePoint()})" : default)));

            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Exception))
                builder.AddExceptionHandle((ex, client)
                    => logger.Append(LoggerLevel.Info, $"{prefix}Exception error handle - {ex}"));

            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Send))
                builder.AddBaseSendHandle((client, pid, len, stackTrace) =>
                {
                    var name = getNameSendPacket(pid);

                    if (name != default)
                        name = $"({name})";

                    logger.Append(LoggerLevel.Info,
                        $"{prefix}Send packet {name}{pid}"
                        + (handleOptions.HasFlag(DefaultEventHandlersEnum.DisplayEndPoint) ? $" to {client.GetRemotePoint()}" : default)
                        + (handleOptions.HasFlag(DefaultEventHandlersEnum.HasSendStackTrace) ? $" {stackTrace}" : default));
                });


            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Receive))
                builder.AddBaseReceiveHandle((client, pid, len) =>
                {
                    var name = getNameReceivePacket(pid);

                    if (name != default)
                        name = $"({name})";

                    logger.Append(LoggerLevel.Info,
                        $"{prefix}Receive packet {name}{pid}"
                        + (handleOptions.HasFlag(DefaultEventHandlersEnum.DisplayEndPoint) ? $" from {client.GetRemotePoint()}" : default));
                });
        }
    }

    [Flags]
    public enum DefaultEventHandlersEnum
    {
        Disconnect = 1,
        Connect = 2,
        Send = 4,
        HasSendStackTrace = 8,
        Receive = 16,
        Exception = 32,
        DisplayEndPoint = 64,
        All = int.MaxValue
    }
}
