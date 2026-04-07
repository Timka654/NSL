#if NSL_LIBRARY
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Pipeline
{
    /// <summary>
    /// Per-channel (per-packetId) middleware pipeline builder.
    /// Registers receive and send middleware layers; the terminal wraps the existing
    /// <see cref="CoreOptions.PacketHandle"/> so backward-compatible handlers continue to work.
    /// </summary>
    public class ChannelPipelineBuilder
    {
        public ushort ChannelId { get; }

        private readonly List<(IChannelReceiveMiddleware mw, int offset)> _recvMiddlewares = new List<(IChannelReceiveMiddleware, int)>();
        private readonly List<(IChannelSendMiddleware mw, int offset)> _sendMiddlewares = new List<(IChannelSendMiddleware, int)>();

        private int _totalReceiveHeaderSize = 0;
        private int _totalSendHeaderSize = 0;

        private CoreOptions.PacketHandle _terminal;

        public ChannelPipelineBuilder(ushort channelId)
        {
            ChannelId = channelId;
        }

        // ── Middleware registration ──────────────────────────────────────────

        public ChannelPipelineBuilder UseReceive(IChannelReceiveMiddleware middleware)
        {
            _recvMiddlewares.Add((middleware, _totalReceiveHeaderSize));
            _totalReceiveHeaderSize += middleware.ReceiveHeaderSize;
            return this;
        }

        public ChannelPipelineBuilder UseSend(IChannelSendMiddleware middleware)
        {
            _sendMiddlewares.Add((middleware, _totalSendHeaderSize));
            _totalSendHeaderSize += middleware.SendHeaderSize;
            return this;
        }

        // ── Terminal ─────────────────────────────────────────────────────────

        internal ChannelPipelineBuilder SetTerminal(CoreOptions.PacketHandle handle)
        {
            _terminal = handle;
            return this;
        }

        // ── Build ─────────────────────────────────────────────────────────────

        public ChannelPipeline Build()
        {
            var receiveChain = BuildReceiveChain();
            var sendChain = BuildSendChain();

            return new ChannelPipeline(
                ChannelId,
                _totalReceiveHeaderSize,
                _totalSendHeaderSize,
                receiveChain,
                sendChain);
        }

        // ── Internal: receive chain construction ─────────────────────────────

        private PacketReceiveDelegate BuildReceiveChain()
        {
            PacketReceiveDelegate chain = BuildReceiveTerminal();

            // Build from innermost (last registered) to outermost (first registered) so that
            // the outermost middleware runs first (ASP.NET-style pipeline order).
            for (int i = _recvMiddlewares.Count - 1; i >= 0; i--)
            {
                var (mw, offset) = _recvMiddlewares[i];
                var size = mw.ReceiveHeaderSize;
                var next = chain;
                chain = ctx =>
                {
                    ctx.MiddlewareHeader = ctx.FullChannelHeader.Slice(offset, size);
                    return mw.InvokeAsync(ctx, next);
                };
            }

            return chain;
        }

        private PacketReceiveDelegate BuildReceiveTerminal()
        {
            if (_terminal == null)
                return _ => default(ValueTask);

            var handle = _terminal;
            return ctx =>
            {
                var rBuff = CreateInputPacketBuffer(ctx);
                try
                {
                    handle(ctx.Connection, rBuff);
                }
                catch (Exception ex)
                {
                    ctx.Connection.Network?.Options?.CallExceptionEvent(ex, ctx.Connection);
                }

                if (!rBuff.ManualDisposing)
                    rBuff.Dispose();

                return default(ValueTask);
            };
        }

        /// <summary>
        /// Creates an <see cref="InputPacketBuffer"/> from the pipeline receive context.
        /// The virtual packetLength reported to the handler only counts the body (not channel headers),
        /// so existing handlers see the same DataLength they expect.
        /// </summary>
        private static InputPacketBuffer CreateInputPacketBuffer(ReceiveChannelContext ctx)
        {
            var bodyLen = ctx.Body.Length;
            var pool = ArrayPool<byte>.Shared;

            // Reconstruct a 7-byte base header where packetLength only covers body (not channel headers).
            Span<byte> headerBuf = stackalloc byte[InputPacketBuffer.DefaultHeaderLength];
            BinaryPrimitives.WriteInt32LittleEndian(headerBuf, InputPacketBuffer.DefaultHeaderLength + bodyLen);
            BinaryPrimitives.WriteUInt16LittleEndian(headerBuf.Slice(4), ctx.PacketId);

            var rBuff = new InputPacketBuffer(headerBuf);

            var data = pool.Rent(bodyLen);
            if (bodyLen > 0)
                ctx.Body.CopyTo(data);
            rBuff.SetData(data);
            rBuff.OnDispose += b => pool.Return(b.Data);

            return rBuff;
        }

        // ── Internal: send chain construction ────────────────────────────────

        private PacketSendDelegate BuildSendChain()
        {
            // The actual "send" happens in ChannelPipeline.SendAsync after the chain runs.
            // The terminal here is a no-op; each middleware writes to its MiddlewareHeader slice.
            PacketSendDelegate chain = _ => default(ValueTask);

            for (int i = _sendMiddlewares.Count - 1; i >= 0; i--)
            {
                var (mw, offset) = _sendMiddlewares[i];
                var size = mw.SendHeaderSize;
                var next = chain;
                chain = ctx =>
                {
                    ctx.MiddlewareHeader = ctx.FullChannelHeader.Slice(offset, size);
                    return mw.InvokeAsync(ctx, next);
                };
            }

            return chain;
        }
    }
}
#endif
