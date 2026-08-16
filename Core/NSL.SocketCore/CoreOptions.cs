using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Cipher;
using NSL.SocketCore.Utils.Logger;
#if NSL_LIBRARY
using NSL.SocketCore.Utils.Pipeline;
#endif
using NSL.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NSL.SocketCore
{
    public interface ITypedCoreOptions<TConnection>
        where TConnection : BaseNetworkConnection, new()
    {
        void AddHandle(ushort pid, Action<TConnection, InputPacketBuffer> handle);
    }

    /// <summary>
    /// Common registration interface shared by <see cref="CoreOptions"/> and pipeline middleware
    /// (e.g. <c>PacketHandleRouterMiddleware</c>). Allows code generators to register handlers
    /// into multiple stores with a single call.
    /// </summary>
    public interface IPacketHandleRegistry
    {
        bool AddPacketHandle(ushort packetId, CoreOptions.PacketHandle handle);
        bool AddPacketHandle(ushort packetId, IPacket packet);
        bool AddAsyncPacketHandle(ushort packetId, Func<BaseNetworkConnection, InputPacketBuffer, Task> handle);
    }

    public class TypedCoreOptions<TConnection> : CoreOptions, ITypedCoreOptions<TConnection>
        where TConnection : BaseNetworkConnection, new()
    {
        public void AddHandle(ushort pid, Action<TConnection, InputPacketBuffer> handle)
        {
            base.AddPacketHandle(pid, (c, buf) => handle((TConnection)c, buf));
        }
    }

    public class CoreOptions : IPacketHandleRegistry
    {
        public ObjectBag ObjectBag { get; } = new ObjectBag();

        /// <summary>
        /// DI service provider — набрасывается через билдер (<c>WithServices</c>) или вручную.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        public IBasicLogger HelperLogger { get; set; }

        public virtual AddressFamily AddressFamily { get; set; } = AddressFamily.Unspecified;

        public virtual ProtocolType ProtocolType { get; set; } = ProtocolType.Unspecified;

        public int ReceiveBufferSize
        {
            get => receiveBufferSize;
            set
            {
                if (value % 2 > 0)
                    throw new InvalidOperationException($"receiveBufferSize must have value % 2");
                if (value < InputPacketBuffer.DefaultHeaderLength)
                    throw new InvalidOperationException($"receiveBufferSize cannot set value less 7");
                receiveBufferSize = value;
            }
        }
        private int receiveBufferSize = 1024;

        public int MaxReceiveBufferSize { get; set; } = int.MaxValue;

        public uint SegmentSize { get; set; } = 1 * 1024;

        private IPacketCipher inputCipher = new PacketNoneCipher();
        public IPacketCipher InputCipher
        {
            get => inputCipher;
            set
            {
                if (inputCipher != null && inputCipher != value) inputCipher.Dispose();
                inputCipher = value ?? new PacketNoneCipher();
            }
        }

        private IPacketCipher outputCipher = new PacketNoneCipher();
        public IPacketCipher OutputCipher
        {
            get => outputCipher;
            set
            {
                if (outputCipher != null && outputCipher != value) outputCipher.Dispose();
                outputCipher = value ?? new PacketNoneCipher();
            }
        }

        // ── Connection factory ───────────────────────────────────────────────
        /// <summary>
        /// Делегат создания экземпляра данных соединения.
        /// Устанавливается автоматически в ServerOptions/ClientOptions через new() или вручную.
        /// </summary>
        public Func<BaseNetworkConnection> ConnectionFactory { get; set; }

        // ── Packet dispatch ──────────────────────────────────────────────────
        public delegate void PacketHandle(BaseNetworkConnection client, InputPacketBuffer data);

        protected Dictionary<ushort, IPacket> Packets = new Dictionary<ushort, IPacket>();
        protected Dictionary<ushort, PacketHandle> PacketHandles = new Dictionary<ushort, PacketHandle>();

#if NSL_LIBRARY
        // ── Channel pipelines ────────────────────────────────────────────────
        private Dictionary<ushort, ChannelPipelineBuilder> _channelBuilders = new Dictionary<ushort, ChannelPipelineBuilder>();

        /// <summary>
        /// Returns the <see cref="ChannelPipelineBuilder"/> for the given channel id (= packetId),
        /// creating it if it does not yet exist. Use this to attach middleware to a channel:
        /// <code>
        /// options.GetOrCreateChannel(MyPid).UseReceive(myMw).UseSend(myMw);
        /// </code>
        /// </summary>
        public ChannelPipelineBuilder GetOrCreateChannel(ushort channelId)
        {
            if (!_channelBuilders.TryGetValue(channelId, out var builder))
                _channelBuilders[channelId] = builder = new ChannelPipelineBuilder(channelId);
            return builder;
        }

        /// <summary>
        /// Builds all registered channel pipelines and returns a per-connection snapshot.
        /// Called once per connection setup (e.g. in the transport's RunReceive).
        /// </summary>
        public Dictionary<ushort, ChannelPipeline> GetChannelPipelineMap()
        {
            var result = new Dictionary<ushort, ChannelPipeline>(_channelBuilders.Count);
            foreach (var entry in _channelBuilders)
                result[entry.Key] = entry.Value.Build();
            return result;
        }
#endif

        public Dictionary<ushort, PacketHandle> GetHandleMap()
            => new Dictionary<ushort, PacketHandle>(PacketHandles);

        public bool AddPacketHandle(ushort packetId, IPacket packet)
        {
            if (PacketHandles.ContainsKey(packetId)) return false;
            Packets[packetId] = packet;
            PacketHandles[packetId] = packet.Receive;
#if NSL_LIBRARY
            GetOrCreateChannel(packetId).SetTerminal(packet.Receive);
#endif
            return true;
        }

        public bool AddPacketHandle(ushort packetId, PacketHandle handle)
        {
            if (PacketHandles.ContainsKey(packetId)) return false;
            PacketHandles[packetId] = handle;
#if NSL_LIBRARY
            GetOrCreateChannel(packetId).SetTerminal(handle);
#endif
            return true;
        }

        public bool AddAsyncPacketHandle(ushort packetId, Func<BaseNetworkConnection, InputPacketBuffer, Task> handle)
            => AddPacketHandle(packetId, (client, input) =>
            {
                input.ManualDisposing = true;
                Task.Run(async () =>
                {
                    try { await handle(client, input); }
                    catch (Exception ex) { CallExceptionEvent(ex, client); }
                    if (input.AsyncDisposing) input.Dispose();
                });
            });

        public IPacket GetPacket(ushort packetId)
        {
            Packets.TryGetValue(packetId, out var result);
            return result;
        }

        // ── Events (non-generic, BaseNetworkConnection) ──────────────────────
        public delegate void ExceptionHandle(Exception ex, BaseNetworkConnection client);
        public delegate void ClientConnect(BaseNetworkConnection client);
        public delegate Task ClientConnectAsync(BaseNetworkConnection client);
        public delegate void ClientDisconnect(BaseNetworkConnection client);
        public delegate Task ClientDisconnectAsync(BaseNetworkConnection client);
        public delegate void ReceivePacketHandle(BaseNetworkConnection client, ushort pid, int len);
        public delegate void SendPacketHandle(BaseNetworkConnection client, ushort pid, int len, string stackTrace);

        public event ExceptionHandle OnExceptionEvent = (ex, c) => { };
        public event ClientConnect OnClientConnectEvent = c => { };
        public event ClientConnectAsync OnClientConnectAsyncEvent = c => Task.CompletedTask;
        public event ClientDisconnect OnClientDisconnectEvent = c => { };
        public event ClientDisconnectAsync OnClientDisconnectAsyncEvent = c => Task.CompletedTask;
        public event ReceivePacketHandle OnReceivePacket = (c, p, i) => { };
        public event SendPacketHandle OnSendPacket = (c, p, i, st) => { };

        public void CallReceivePacketEvent(BaseNetworkConnection client, ushort pid, int len)
            => OnReceivePacket(client, pid, len);

        public void CallSendPacketEvent(BaseNetworkConnection client, ushort pid, int len, string stackTrace)
            => OnSendPacket(client, pid, len, stackTrace);

        public virtual void CallExceptionEvent(Exception ex, BaseNetworkConnection client)
            => OnExceptionEvent(ex, client);

        public virtual void CallClientConnectEvent(BaseNetworkConnection client)
        {
            OnClientConnectEvent(client);
            Task.Run(() => OnClientConnectAsyncEvent.InvokeAsync(t => t(client)));
        }

        public virtual void CallClientDisconnectEvent(BaseNetworkConnection client)
        {
            if (client == null) return;
            client.DisconnectTime = DateTime.UtcNow;
            OnClientDisconnectEvent?.Invoke(client);
            Task.Run(() => OnClientDisconnectAsyncEvent.InvokeAsync(t => t(client)));
        }

        // ── Client-side lifecycle (base, non-typed) ──────────────────────────
        public virtual BaseNetworkConnection ClientData { get; protected set; }

        public virtual void InitializeClient(BaseNetworkConnection newClientData) => ClientData = newClientData;

        public virtual void RunClientConnect() => CallClientConnectEvent(ClientData);

        public virtual void RunClientDisconnect() => CallClientDisconnectEvent(ClientData);

        public virtual void RunException(Exception ex) => CallExceptionEvent(ex, ClientData);
    }

    /// <summary>
    /// Typed extension methods for <see cref="CoreOptions"/>.
    /// Allows registering handlers with typed TClient parameter (cast internally).
    /// </summary>
    public static class CoreOptionsExtensions
    {
        public static bool AddPacketHandle<TClient>(this CoreOptions options, ushort packetId, Action<TClient, InputPacketBuffer> handle)
            where TClient : BaseNetworkConnection
            => options.AddPacketHandle(packetId, (c, buf) => handle((TClient)c, buf));

        public static bool AddPacketHandle<TClient>(this IPacketHandleRegistry registry, ushort packetId, Action<TClient, InputPacketBuffer> handle)
            where TClient : BaseNetworkConnection
            => registry.AddPacketHandle(packetId, (c, buf) => handle((TClient)c, buf));

        public static bool AddAsyncPacketHandle<TClient>(this IPacketHandleRegistry registry, ushort packetId, Func<TClient, InputPacketBuffer, Task> handle)
            where TClient : BaseNetworkConnection
            => registry.AddAsyncPacketHandle(packetId, (c, buf) => handle((TClient)c, buf));
    }
}
