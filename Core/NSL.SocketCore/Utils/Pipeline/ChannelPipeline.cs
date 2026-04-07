#if NSL_LIBRARY
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Buffers.Binary;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Pipeline
{
    /// <summary>
    /// Compiled per-channel (per-packetId) pipeline.
    /// Instances are created by <see cref="ChannelPipelineBuilder.Build"/> and stored per-connection
    /// so both receive and send chains are resolved in O(1).
    /// </summary>
    public class ChannelPipeline
    {
        public ushort ChannelId { get; }

        /// <summary>
        /// Total bytes reserved by registered receive middleware in the channel header region
        /// (immediately after the 7-byte base header, before the body).
        /// The transport must read exactly this many bytes as part of the channel header.
        /// </summary>
        public int TotalReceiveHeaderSize { get; }

        /// <summary>
        /// Total bytes reserved by registered send middleware in the channel header region
        /// of outgoing packets.
        /// </summary>
        public int TotalSendHeaderSize { get; }

        private readonly PacketReceiveDelegate _receiveChain;
        private readonly PacketSendDelegate _sendChain;

        internal ChannelPipeline(
            ushort channelId,
            int totalReceiveHeaderSize,
            int totalSendHeaderSize,
            PacketReceiveDelegate receiveChain,
            PacketSendDelegate sendChain)
        {
            ChannelId = channelId;
            TotalReceiveHeaderSize = totalReceiveHeaderSize;
            TotalSendHeaderSize = totalSendHeaderSize;
            _receiveChain = receiveChain;
            _sendChain = sendChain;
        }

        // ── Receive ──────────────────────────────────────────────────────────

        /// <summary>
        /// Dispatches a received packet through the middleware chain.
        /// The channel header and body slices come from the transport's receive buffer.
        /// </summary>
        public ValueTask HandleReceiveAsync(ReceiveChannelContext ctx)
            => _receiveChain(ctx);

        // ── Send ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the outgoing packet buffer, runs the send middleware chain (so each middleware
        /// can write into its reserved channel header slice), then hands the final byte array to
        /// the transport (which applies the output cipher and puts it in the send queue).
        /// </summary>
        public async ValueTask SendAsync(BaseNetworkConnection connection, PacketBodyBuffer body)
        {
            var bodyLen = (int)body.Length;
            var totalLen = OutputPacketBuffer.DefaultHeaderLength + TotalSendHeaderSize + bodyLen;
            var packetBuf = new byte[totalLen];

            // Copy body into the packet buffer at the correct offset
            int bodyDst = OutputPacketBuffer.DefaultHeaderLength + TotalSendHeaderSize;
            if (bodyLen > 0)
            {
                if (body.TryGetBuffer(out var seg))
                    System.Buffer.BlockCopy(seg.Array, seg.Offset, packetBuf, bodyDst, bodyLen);
                else
                    System.Buffer.BlockCopy(body.ToArray(), 0, packetBuf, bodyDst, bodyLen);
            }

            // Write base header — length includes channel headers and body
            BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(packetBuf, 0, 4), totalLen);
            BinaryPrimitives.WriteUInt16LittleEndian(new Span<byte>(packetBuf, 4, 2), ChannelId);
            // Byte [6] is the optional CRC; written after middleware so length is final
            packetBuf[6] = (byte)((totalLen + ChannelId) % 255);

            // Run send middleware — each layer writes into its reserved channel header slice
            if (TotalSendHeaderSize > 0 || _sendChain != null)
            {
                var channelHeaderRegion = new Memory<byte>(packetBuf, OutputPacketBuffer.DefaultHeaderLength, TotalSendHeaderSize);
                var bodyRegion = new Memory<byte>(packetBuf, bodyDst, bodyLen);
                var ctx = new SendChannelContext(connection, ChannelId, channelHeaderRegion, bodyRegion);
                await _sendChain(ctx);
            }

            // Hand off to transport; the transport's sendBuf applies the output cipher
            connection.Network.Send(packetBuf);
        }
    }
}
#endif
