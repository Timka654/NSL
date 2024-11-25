using NSL.EndPointBuilder;
using NSL.Logger;
using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Extensions.Buffer.Interface;
using NSL.SocketCore.Extensions.Packet;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using NSL.SocketCore.Utils.Logger;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Net.Sockets;
using System.Reflection;
using static NSL.SocketCore.Extensions.Buffer.RequestExtensions;

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

        [Obsolete("Renamed to AddResponsePacketHandle", true)]
        public static void AddReceivePacketHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, ushort packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient, new()
            => AddResponsePacketHandle(builder, packetId, handler);

        public static void AddResponsePacketHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, ushort packetId, Func<TClient, IResponsibleProcessor> handler)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().AddResponsePacketHandle(packetId, handler);
        }

        [Obsolete("Renamed to AddResponsePacketHandle", true)]
        public static void AddReceivePacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, Func<TClient, IResponsibleProcessor> handler)
            where TEnum : struct, IConvertible
            where TClient : INetworkClient, new()
            => AddResponsePacketHandle(builder, packetId, handler);

        public static void AddResponsePacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, Func<TClient, IResponsibleProcessor> handler)
            where TEnum : struct, IConvertible
            where TClient : INetworkClient, new()
        {
            AddResponsePacketHandle(builder, packetId.ToUInt16(null), handler);
        }

        public static void AddRequestPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, RequestPacketHandle<TClient> packet) where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddRequestPacketHandle(packetId, packet);

        public static void AddRequestPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, RequestPacketHandle2<TClient> packet, ushort responsePacketId = 1) where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddRequestPacketHandle(packetId, packet, responsePacketId);

        public static void AddAsyncRequestPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, RequestPacketAsyncHandle<TClient> packet) where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddAsyncRequestPacketHandle(packetId, packet);

        public static void AddAsyncRequestPacketHandle<TClient, TEnum>(this IOptionableEndPointBuilder<TClient> builder, TEnum packetId, RequestPacketAsyncHandle2<TClient> packet, ushort responsePacketId = 1) where TClient : INetworkClient, new() where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddAsyncRequestPacketHandle(packetId, packet, responsePacketId);

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

        public static void AddSendHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.SendPacketHandle handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnSendPacket += handle;
        }

        public static void AddReceiveHandle<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.ReceivePacketHandle handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnReceivePacket += handle;
        }

        public static void AddClientObjectBag<TClient>(this IOptionableEndPointBuilder<TClient> builder)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnClientConnectEvent += c => c.InitializeObjectBag();
        }

        public static void SetLogger<TClient>(this IOptionableEndPointBuilder<TClient> builder, IBasicLogger logger)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().HelperLogger = logger;
        }


        #region DefaultHandles

        private static Func<ushort, string> defaultGetNamePacketHandle = pid => default;

        #endregion

        public static void AddDefaultEventHandlers<TClient>(this IOptionableEndPointBuilder<TClient> builder,
            string prefix = default,
            DefaultEventHandlersEnum handleOptions = DefaultEventHandlersEnum.All,
            Func<ushort, string> getNameSendPacket = default,
            Func<ushort, string> getNameReceivePacket = default)
            where TClient : INetworkClient, new()
        {
            var logger = builder
               .GetCoreOptions()
               .HelperLogger;

            if (!string.IsNullOrWhiteSpace(prefix))
                logger = new PrefixableLoggerProxy(logger, prefix);

            builder.AddDefaultEventHandlers(logger, handleOptions, getNameSendPacket, getNameReceivePacket);
        }

        public static void AddDefaultEventHandlers<TClient>(this IOptionableEndPointBuilder<TClient> builder,
            IBasicLogger logger,
            DefaultEventHandlersEnum handleOptions = DefaultEventHandlersEnum.All,
            Func<ushort, string> getNameSendPacket = default,
            Func<ushort, string> getNameReceivePacket = default)
            where TClient : INetworkClient, new()
        {
            if (logger == default)
                throw new InvalidOperationException($"{nameof(CoreOptions.HelperLogger)} must be installed before invoke this method");

            if (getNameSendPacket == default)
                getNameSendPacket = defaultGetNamePacketHandle;

            if (getNameReceivePacket == default)
                getNameReceivePacket = defaultGetNamePacketHandle;


            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Connect))
                builder.AddConnectHandle(client =>
                {
                    try
                    {
                        string msg = $"Success connected";

                        if (handleOptions.HasFlag(DefaultEventHandlersEnum.DisplayEndPoint) && client?.Network != null)
                            msg += $"({client.Network.GetRemotePoint()})";

                        logger.Append(LoggerLevel.Info, msg);
                    }
                    catch { }
                });

            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Disconnect))
                builder.AddDisconnectHandle(client =>
                {
                    try
                    {
                        string msg = $"Success disconnected";

                        if (handleOptions.HasFlag(DefaultEventHandlersEnum.DisplayEndPoint) && client?.Network != null)
                            msg += $"({client.Network.GetRemotePoint()})";

                        logger.Append(LoggerLevel.Info, msg);
                    }
                    catch { }
                });

            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Exception))
                builder.AddExceptionHandle((ex, client) =>
                {
                    if (ex is ConnectionLostException)
                        return;

                    logger.Append(LoggerLevel.Error, $"Exception error handle - {ex}");
                });

            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Send))
                builder.AddSendHandle((client, pid, len, stackTrace) =>
                {
                    if (handleOptions.HasFlag(DefaultEventHandlersEnum.ExcludeSystemPid) && OutputPacketBuffer.IsSystemPID(pid))
                        return;

                    var msg = getNameSendPacket(pid);

                    if (msg != default)
                        msg = $"({msg})";

                    msg = $"Send packet {pid}{msg}";

                    try
                    {
                        if (handleOptions.HasFlag(DefaultEventHandlersEnum.DisplayEndPoint) && client?.Network != null)
                            msg += $" to {client?.Network?.GetRemotePoint()}";

                        if (handleOptions.HasFlag(DefaultEventHandlersEnum.HasSendStackTrace))
                            msg += $" {stackTrace}";

                        logger.Append(LoggerLevel.Info, msg);
                    }
                    catch { }
                });


            if (handleOptions.HasFlag(DefaultEventHandlersEnum.Receive))
                builder.AddReceiveHandle((client, pid, len) =>
                {
                    if (handleOptions.HasFlag(DefaultEventHandlersEnum.ExcludeSystemPid) && InputPacketBuffer.IsSystemPID(pid))
                        return;

                    var msg = getNameReceivePacket(pid);

                    if (msg != default)
                        msg = $"({msg})";

                    msg = $"Receive packet {pid}{msg}";

                    try
                    {
                        if (handleOptions.HasFlag(DefaultEventHandlersEnum.DisplayEndPoint) && client?.Network != null)
                            msg += $" from {client?.Network?.GetRemotePoint()}";

                        logger.Append(LoggerLevel.Info, msg);
                    }
                    catch { }
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
        ExcludeSystemPid = 128,
        All = int.MaxValue
    }
}
