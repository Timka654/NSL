#if NSL_LIBRARY
using System;
using System.Buffers.Binary;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils.Pipeline.Middleware
{
    /// <summary>
    /// Channel middleware that appends and validates a CRC-16/CCITT-FALSE checksum.
    /// Reserves 2 bytes in the channel header region.
    /// <para>Send: computes CRC over the body and writes it into the header slice.</para>
    /// <para>Receive: reads CRC from the header slice and validates it against the body; throws on mismatch.</para>
    /// </summary>
    public class CrcChannelMiddleware : IChannelReceiveMiddleware, IChannelSendMiddleware
    {
        public int ReceiveHeaderSize => 2;
        public int SendHeaderSize    => 2;

        public ValueTask InvokeAsync(ReceiveChannelContext ctx, PacketReceiveDelegate next)
        {
            var expected = BinaryPrimitives.ReadUInt16LittleEndian(ctx.MiddlewareHeader.Span);
            var actual   = ComputeCrc16(ctx.Body.Span);
            if (expected != actual)
                throw new InvalidOperationException(
                    $"[CRC] PacketId={ctx.PacketId} expected=0x{expected:X4} actual=0x{actual:X4}");
            return next(ctx);
        }

        public ValueTask InvokeAsync(SendChannelContext ctx, PacketSendDelegate next)
        {
            var crc = ComputeCrc16(ctx.Body.Span);
            BinaryPrimitives.WriteUInt16LittleEndian(ctx.MiddlewareHeader.Span, crc);
            return next(ctx);
        }

        /// <summary>CRC-16/CCITT-FALSE: Poly=0x1021, Init=0xFFFF, RefIn=false, RefOut=false, XorOut=0x0000.</summary>
        public static ushort ComputeCrc16(ReadOnlySpan<byte> data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                    crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
            }
            return crc;
        }
    }
}
#endif
