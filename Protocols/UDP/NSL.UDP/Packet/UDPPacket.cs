using NSL.UDP.Enums;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace NSL.UDP.Packet
{
    internal class UDPPacket
    {
        public static byte FullHeadByte = (byte)DgramHeadTypeEnum.Full;

        public const byte BaseHeadLen = 8;
        public const byte FullHeadLen = BaseHeadLen + 2;

        protected static byte[] emptySumBytes = new byte[] { 0, 0 };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort GetChecksum(byte[] buffer)
			=> GetChecksum((Span<byte>)buffer);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort GetChecksum(Span<byte> buffer)
		{
			emptySumBytes.CopyTo(buffer[2..]);

#if NET5_0_OR_GREATER
			return (ushort)(SHA256.HashData(buffer.ToArray()).Sum(x => x) % ushort.MaxValue);
#endif
			using (var hasher = SHA256.Create())
			{
				return (ushort)(hasher.ComputeHash(buffer.ToArray()).Sum(x => x) % ushort.MaxValue);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadISFull(Span<byte> buffer) => (DgramHeadTypeEnum)buffer[0] == DgramHeadTypeEnum.Full; // end offset = 1

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UDPChannelEnum ReadChannel(Span<byte> buffer) => (UDPChannelEnum)buffer[1]; // end offset = 2

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort ReadChecksum(Span<byte> buffer) => BitConverter.ToUInt16(buffer[2..]); // end offset = 4

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint ReadPID(Span<byte> buffer) => BitConverter.ToUInt32(buffer[4..]); // end offset = 8


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> CreateFull(byte[] pidBytes, byte channelByte, byte[] data)
        {
            var lpBuf = (Memory<byte>)new byte[FullHeadLen + data.Length];

            lpBuf.Span[0] = FullHeadByte;

            lpBuf.Span[1] = channelByte;

            // 2..4 checksum

            pidBytes
                .CopyTo(lpBuf[4..]);

            BitConverter.GetBytes((ushort)0)
                .CopyTo(lpBuf[8..]);

            data
                .CopyTo(lpBuf[10..]);


            var lpBufRes = lpBuf.ToArray();

            var cs = UDPPacket.GetChecksum(lpBufRes);

            BitConverter.GetBytes(cs)
                .CopyTo(lpBuf[2..]);

            return lpBuf;
        }
    }
}
