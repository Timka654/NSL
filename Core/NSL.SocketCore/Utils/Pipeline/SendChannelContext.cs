#if NSL_LIBRARY
using System;
using System.Collections.Generic;

namespace NSL.SocketCore.Utils.Pipeline
{
    public class SendChannelContext
    {
        /// <summary>The connection that will send this packet.</summary>
        public BaseNetworkConnection Connection { get; }

        /// <summary>Packet id to be written into the base header.</summary>
        public ushort PacketId { get; }

        /// <summary>
        /// Current middleware's writable slice of the channel header region.
        /// Set by the pipeline engine before each middleware is invoked.
        /// </summary>
        public Memory<byte> MiddlewareHeader { get; internal set; }

        /// <summary>
        /// Full channel header region backing the outgoing packet buffer.
        /// Used internally by the pipeline engine to compute per-middleware slices.
        /// </summary>
        internal Memory<byte> FullChannelHeader { get; }

        /// <summary>The body bytes (already written into the final packet buffer at the correct offset).</summary>
        public ReadOnlyMemory<byte> Body { get; }

        /// <summary>Per-request data bag for passing state between middleware.</summary>
        public Dictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public SendChannelContext(
            BaseNetworkConnection connection,
            ushort packetId,
            Memory<byte> fullChannelHeader,
            ReadOnlyMemory<byte> body)
        {
            Connection = connection;
            PacketId = packetId;
            FullChannelHeader = fullChannelHeader;
            Body = body;
        }
    }
}
#endif
