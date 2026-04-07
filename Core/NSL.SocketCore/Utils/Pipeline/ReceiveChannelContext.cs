#if NSL_LIBRARY
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Buffers;
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
        /// Creates a <see cref="PacketBodyReader"/> backed by the body of this context.
        /// <c>DataLength</c> equals <see cref="Body"/>.Length and position starts at 0.
        /// No packet header metadata is present (use <see cref="InputPacketBuffer.FromBody"/> if you need it).
        /// The caller is responsible for disposing the returned reader (pool return is hooked on Dispose).
        /// </summary>
        public PacketBodyReader CreateReader()
        {
            var bodyLen = Body.Length;
            var pool = ArrayPool<byte>.Shared;
            var data = pool.Rent(bodyLen);
            if (bodyLen > 0) Body.CopyTo(data);
            var reader = new PacketBodyReader();
            reader.SetData(data, bodyLen);
            reader.OnDispose += _ => pool.Return(data);
            return reader;
        }
    }
}
#endif
