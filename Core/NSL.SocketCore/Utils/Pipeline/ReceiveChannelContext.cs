#if NSL_LIBRARY
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace NSL.SocketCore.Utils.Pipeline
{
    public class ReceiveChannelContext
    {
        /// <summary>The connection that received this packet.</summary>
        public BaseNetworkConnection Connection { get; }

        /// <summary>Packet id parsed from the base 7-byte header.</summary>
        public ushort PacketId { get; }

        /// <summary>Total wire packet length (base header + channel headers + body).</summary>
        public int TotalPacketLength { get; }

        /// <summary>
        /// Current middleware's slice of the channel header buffer.
        /// Set by the pipeline engine to the correct slice before each middleware is invoked.
        /// </summary>
        public Memory<byte> MiddlewareHeader { get; internal set; }

        /// <summary>
        /// Full channel header region (all middleware headers concatenated).
        /// Used internally by the pipeline engine to compute per-middleware slices.
        /// </summary>
        internal Memory<byte> FullChannelHeader { get; }

        /// <summary>Body bytes — everything after the base 7-byte header and all channel headers.</summary>
        public Memory<byte> Body { get; }

        /// <summary>Per-request data bag for passing state between middleware.</summary>
        public Dictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public ReceiveChannelContext(
            BaseNetworkConnection connection,
            ushort packetId,
            int totalPacketLength,
            Memory<byte> fullChannelHeader,
            Memory<byte> body)
        {
            Connection = connection;
            PacketId = packetId;
            TotalPacketLength = totalPacketLength;
            FullChannelHeader = fullChannelHeader;
            Body = body;
        }

        /// <summary>
        /// Creates an <see cref="InputPacketBuffer"/> backed by the body of this context.
        /// The buffer's <c>DataLength</c> equals <see cref="Body.Length"/> and position starts at 0.
        /// The caller is responsible for disposing the returned buffer (or setting <c>ManualDisposing</c>).
        /// </summary>
        public InputPacketBuffer CreateReader()
        {
            var bodyLen = Body.Length;
            var pool = ArrayPool<byte>.Shared;

            Span<byte> headerBuf = stackalloc byte[InputPacketBuffer.DefaultHeaderLength];
            BinaryPrimitives.WriteInt32LittleEndian(headerBuf, InputPacketBuffer.DefaultHeaderLength + bodyLen);
            BinaryPrimitives.WriteUInt16LittleEndian(headerBuf.Slice(4), PacketId);

            var rBuff = new InputPacketBuffer(headerBuf);
            var data = pool.Rent(bodyLen);
            if (bodyLen > 0)
                Body.CopyTo(data);
            rBuff.SetData(data);
            rBuff.OnDispose += b => pool.Return(b.Data);
            return rBuff;
        }
    }
}
#endif
