using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Request;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.UDP
{
    /// <summary>
    /// A <see cref="RequestProcessor"/> bound to a specific UDP channel.
    /// Multiple instances with different <see cref="Channel"/> values can share the same
    /// <see cref="IRequestHub"/> so that all response packets are routed through a single
    /// registered handler, while requests are dispatched over different channels.
    /// </summary>
    public class UdpRequestProcessor : RequestProcessor
    {
        /// <summary>The UDP channel used when sending requests via this processor.</summary>
        public UDPChannelEnum Channel { get; }

        /// <summary>
        /// Create a processor bound to <paramref name="channel"/>, sharing the given <paramref name="hub"/>.
        /// </summary>
        public UdpRequestProcessor(INetworkClient client, UDPChannelEnum channel, IRequestHub hub)
            : base(client, hub)
        {
            Channel = channel;
        }

        /// <summary>
        /// Create a processor bound to <paramref name="channel"/> with its own private hub.
        /// </summary>
        public UdpRequestProcessor(INetworkClient client, UDPChannelEnum channel)
            : base(client)
        {
            Channel = channel;
        }

        // ── Buffer factory helpers ────────────────────────────────────────────────

        /// <summary>
        /// Create a <see cref="DgramRequestPacketBuffer"/> pre-configured with
        /// this processor's default <see cref="Channel"/>.
        /// </summary>
        public DgramRequestPacketBuffer CreateBuffer(ushort packetId, int len = 48)
            => new DgramRequestPacketBuffer(len) { Channel = Channel }.WithPid(packetId);

        /// <summary>
        /// Create a <see cref="DgramRequestPacketBuffer"/> pre-configured with
        /// this processor's default <see cref="Channel"/>.
        /// </summary>
        public DgramRequestPacketBuffer CreateBuffer<TEnum>(TEnum packetId, int len = 48)
            where TEnum : struct, System.Enum, System.IConvertible
            => new DgramRequestPacketBuffer(len) { Channel = Channel }.WithPid(packetId);

        /// <summary>
        /// Create a <see cref="DgramRequestPacketBuffer"/> using an explicit <paramref name="channel"/>,
        /// overriding the processor's default for this single request.
        /// </summary>
        public DgramRequestPacketBuffer CreateBuffer(ushort packetId, UDPChannelEnum channel, int len = 48)
            => new DgramRequestPacketBuffer(len) { Channel = channel }.WithPid(packetId);

        // ── Typed SendRequest / SendRequestAsync overloads ───────────────────────

        /// <inheritdoc cref="RequestProcessor.SendRequest"/>
        public Guid SendRequest(DgramRequestPacketBuffer buffer, Func<InputPacketBuffer, bool> onResponse, CancellationToken cancellationToken, bool disposeOnSend = true)
            => base.SendRequest(buffer, onResponse, cancellationToken, disposeOnSend);

        /// <inheritdoc cref="RequestProcessor.SendRequest"/>
        public Guid SendRequest(DgramRequestPacketBuffer buffer, Func<InputPacketBuffer, bool> onResponse, bool disposeOnSend = true)
            => base.SendRequest(buffer, onResponse, CancellationToken.None, disposeOnSend);

        /// <inheritdoc cref="RequestProcessor.SendRequestAsync(RequestPacketBuffer, Func{InputPacketBuffer, Task{bool}}, bool)"/>
        public Task SendRequestAsync(DgramRequestPacketBuffer buffer, Func<InputPacketBuffer, Task<bool>> onResult, bool disposeOnSend = true)
            => base.SendRequestAsync(buffer, onResult, disposeOnSend);

        /// <inheritdoc cref="RequestProcessor.SendRequestAsync(RequestPacketBuffer, Func{InputPacketBuffer, Task{bool}}, CancellationToken, bool)"/>
        public Task SendRequestAsync(DgramRequestPacketBuffer buffer, Func<InputPacketBuffer, Task<bool>> onResult, CancellationToken cancellationToken, bool disposeOnSend = true)
            => base.SendRequestAsync(buffer, onResult, cancellationToken, disposeOnSend);
    }
}
