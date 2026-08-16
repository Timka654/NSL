using Microsoft.Extensions.DependencyInjection;
using NSL.EndPointBuilder;
using NSL.SocketCore;
using NSL.SocketCore.Network;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Exceptions;
using NSL.SocketCore.Utils.Logger;
using NSL.SocketCore.Utils.Logger.Enums;
using NSL.SocketCore.Utils.Packet;
using NSL.SocketCore.Utils.Request;
using System;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using static NSL.SocketCore.CoreOptions;
using static NSL.SocketCore.Utils.Request.RequestExtensions;

namespace NSL.SocketCore.Network
{
    public static class Extensions
    {
        public static void AddPacket(this IOptionableEndPointBuilder builder, ushort packetId, IPacket packet)
            => builder.GetCoreOptions().AddPacketHandle(packetId, packet);

        public static void AddPacket<TEnum>(this IOptionableEndPointBuilder builder, TEnum packetId, IPacket packet)
            where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddPacketHandle(packetId.ToUInt16(null), packet);

        public static bool AddPacketHandle<TClient>(this IOptionableEndPointBuilder builder, ushort packetId, Action<TClient, InputPacketBuffer> handle)
            where TClient : BaseNetworkConnection
            => builder.GetCoreOptions().AddPacketHandle(packetId, (c, buf) => handle((TClient)c, buf));

        public static bool AddAsyncPacketHandle<TClient>(this IOptionableEndPointBuilder builder, ushort packetId, Func<TClient, InputPacketBuffer, Task> handle)
            where TClient : BaseNetworkConnection
            => builder.GetCoreOptions().AddAsyncPacketHandle(packetId, (c, buf) => handle((TClient)c, buf));

        public static void AddResponsePacketHandle(this IOptionableEndPointBuilder builder, ushort packetId, Func<BaseNetworkConnection, IResponsibleProcessor> handler)
            => builder.GetCoreOptions().AddResponsePacketHandle(packetId, handler);

        public static void AddResponsePacketHandle<TEnum>(this IOptionableEndPointBuilder builder, TEnum packetId, Func<BaseNetworkConnection, IResponsibleProcessor> handler)
            where TEnum : struct, IConvertible
            => AddResponsePacketHandle(builder, packetId.ToUInt16(null), handler);

        public static void AddRequestPacketHandle<BaseNetworkConnection, TEnum>(this IOptionableEndPointBuilder builder, TEnum packetId, RequestPacketHandle packet)
            where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddRequestPacketHandle(packetId, packet);

        public static void AddRequestPacketHandle<TEnum>(this IOptionableEndPointBuilder builder, TEnum packetId, RequestPacketHandle2 packet, ushort responsePacketId = 1)
            where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddRequestPacketHandle(packetId, packet, responsePacketId);

        public static void AddAsyncRequestPacketHandle<TEnum>(this IOptionableEndPointBuilder builder, TEnum packetId, RequestPacketAsyncHandle packet) where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddAsyncRequestPacketHandle(packetId, packet);

        public static void AddAsyncRequestPacketHandle<TEnum>(this IOptionableEndPointBuilder builder, TEnum packetId, RequestPacketAsyncHandle2 packet, ushort responsePacketId = 1) where TEnum : struct, IConvertible
            => builder.GetCoreOptions().AddAsyncRequestPacketHandle(packetId, packet, responsePacketId);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TPacket">PacketAttribute impl</typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static void LoadPackets<TPacket>(this IOptionableEndPointBuilder builder, Assembly assembly)
            where TPacket : PacketAttribute
            => builder.GetCoreOptions().LoadPackets(assembly, typeof(TPacket), t => (IPacket)Activator.CreateInstance(t));

        /// <summary>
        /// with caling assembly
        /// </summary>
        /// <typeparam name="TPacket">PacketAttribute impl</typeparam>
        /// <returns></returns>
        public static void LoadPackets<TPacket>(this IOptionableEndPointBuilder builder)
            where TPacket : PacketAttribute
            => LoadPackets<TPacket>(builder, Assembly.GetCallingAssembly());

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TPacket">PacketAttribute impl</typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static void LoadPackets(this IOptionableEndPointBuilder builder, Assembly assembly, Type packetAttributeSelectType)
        {
            if (packetAttributeSelectType.IsAssignableFrom(typeof(PacketAttribute)))
                throw new ArgumentOutOfRangeException($"paameter {nameof(packetAttributeSelectType)} must be assgnable from {nameof(PacketAttribute)}");


            builder.GetCoreOptions().LoadPackets(assembly, packetAttributeSelectType, t => (IPacket)Activator.CreateInstance(t));
        }

        /// <summary>
        /// with caling assembly
        /// </summary>
        /// <typeparam name="TPacket">PacketAttribute impl</typeparam>
        /// <returns></returns>
        public static void LoadPackets(this IOptionableEndPointBuilder builder, Type packetAttributeSelectType)
            => LoadPackets(builder, Assembly.GetCallingAssembly(), packetAttributeSelectType);

        public static void WithInputCipher(this IOptionableEndPointBuilder builder, IPacketCipher cipher)
            => builder.GetCoreOptions().InputCipher = cipher;

        public static void WithOutputCipher(this IOptionableEndPointBuilder builder, IPacketCipher cipher)
            => builder.GetCoreOptions().OutputCipher = cipher;

        public static void WithAddressFamily(this IOptionableEndPointBuilder builder, AddressFamily family)
            => builder.GetCoreOptions().AddressFamily = family;

        public static void WithProtocolType(this IOptionableEndPointBuilder builder, ProtocolType type)
            => builder.GetCoreOptions().ProtocolType = type;

        public static void WithBufferSize(this IOptionableEndPointBuilder builder, int size)
            => builder.GetCoreOptions().ReceiveBufferSize = size;

        public static void AddConnectHandle(this IOptionableEndPointBuilder builder, CoreOptions.ClientConnect handle)
            => builder.GetCoreOptions().OnClientConnectEvent += handle;

        public static void AddDisconnectHandle(this IOptionableEndPointBuilder builder, CoreOptions.ClientDisconnect handle)
            => builder.GetCoreOptions().OnClientDisconnectEvent += handle;

        public static void AddExceptionHandle(this IOptionableEndPointBuilder builder, CoreOptions.ExceptionHandle handle)
            => builder.GetCoreOptions().OnExceptionEvent += handle;

        public static void AddConnectAsyncHandle(this IOptionableEndPointBuilder builder, CoreOptions.ClientConnectAsync handle)
            => builder.GetCoreOptions().OnClientConnectAsyncEvent += handle;

        public static void AddDisconnectAsyncHandle(this IOptionableEndPointBuilder builder, CoreOptions.ClientDisconnectAsync handle)
            => builder.GetCoreOptions().OnClientDisconnectAsyncEvent += handle;

        public static void AddSendHandle(this IOptionableEndPointBuilder builder, CoreOptions.SendPacketHandle handle)
            => builder.GetCoreOptions().OnSendPacket += handle;

        public static void AddReceiveHandle(this IOptionableEndPointBuilder builder, ReceivePacketHandle handle)
            => builder.GetCoreOptions().OnReceivePacket += handle;

        public static void AddClientObjectBag(this IOptionableEndPointBuilder builder)
            => builder.GetCoreOptions().OnClientConnectEvent += c => c.InitializeObjectBag();

        /// <summary>
        /// Устанавливает существующий <see cref="IServiceProvider"/> для данного endpoint-а.
        /// Используется когда сервисы уже созданы (например, разделяются с ASP.NET или другим протоколом).
        /// </summary>
        public static void WithServices(this IOptionableEndPointBuilder builder, IServiceProvider serviceProvider)
            => builder.GetCoreOptions().ServiceProvider = serviceProvider;

        /// <summary>
        /// Регистрирует DI-сервисы и создаёт <see cref="IServiceProvider"/> для данного endpoint-а.
        /// Должен вызываться до <c>Build()</c>.
        /// </summary>
        public static void WithServices(this IOptionableEndPointBuilder builder, Action<IServiceCollection> configure)
        {
            var services = new ServiceCollection();
            configure(services);
            builder.GetCoreOptions().ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Регистрирует обработчик connect, который автоматически вызывает <see cref="BaseNetworkConnection.InitializeServiceScope"/>
        /// при подключении клиента. <c>WithServices</c> должен быть вызван ДО этого метода.
        /// </summary>
        public static void AddScopedConnect(this IOptionableEndPointBuilder builder)
        {
            var opts = builder.GetCoreOptions();
            builder.GetCoreOptions().OnClientConnectEvent += c =>
            {
                var provider = opts.ServiceProvider;
                if (provider == null)
                    throw new InvalidOperationException($"ServiceProvider is not configured. Call WithServices before AddScopedConnect.");
                c.InitializeServiceScope(provider);
            };
        }

        public static void SetLogger(this IOptionableEndPointBuilder builder, IBasicLogger logger)
        => builder.GetCoreOptions().HelperLogger = logger;

        /// <summary>
        /// Create <see cref="RequestProcessor"/> with <paramref name="objectKey"/> in Client.ObjectBag and register handle for execute wait receive packet in buffer
        /// </summary>
        public static void ConfigureRequestProcessor<TEnum>(this IOptionableEndPointBuilder builder, TEnum responsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
            where TEnum : struct, IConvertible
            => builder.GetCoreOptions().ConfigureRequestProcessor<TEnum>(responsePacketId, objectKey);

        /// <summary>
        /// Create <see cref="RequestProcessor"/> with <paramref name="objectKey"/> in Client.ObjectBag and register handle for execute wait receive packet in buffer
        /// </summary>
        public static void ConfigureRequestProcessor(this IOptionableEndPointBuilder builder, ushort responsePacketId = RequestProcessor.DefaultResponsePacketId, string objectKey = RequestProcessor.DefaultObjectBagKey)
            => builder.GetCoreOptions().ConfigureRequestProcessor(responsePacketId, objectKey);

        #region DefaultHandles

        private static Func<ushort, string> defaultGetNamePacketHandle = pid => default;

        #endregion

        public static void AddDefaultEventHandlers(this IOptionableEndPointBuilder builder,
            string prefix = default,
            DefaultEventHandlersEnum handleOptions = DefaultEventHandlersEnum.All,
            Func<ushort, string> getNameSendPacket = default,
            Func<ushort, string> getNameReceivePacket = default)
        {
            var logger = builder
               .GetCoreOptions()
               .HelperLogger;

            if (!string.IsNullOrWhiteSpace(prefix))
                logger = new PrefixableLoggerProxy(logger, prefix);

            builder.AddDefaultEventHandlers(logger, handleOptions, getNameSendPacket, getNameReceivePacket);
        }

        public static void AddDefaultEventHandlers(this IOptionableEndPointBuilder builder,
            IBasicLogger logger,
            DefaultEventHandlersEnum handleOptions = DefaultEventHandlersEnum.All,
            Func<ushort, string> getNameSendPacket = default,
            Func<ushort, string> getNameReceivePacket = default)
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
