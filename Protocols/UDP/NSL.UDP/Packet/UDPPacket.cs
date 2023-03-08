using NSL.UDP.Enums;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace NSL.UDP.Packet
{
    internal class UDPPacket
	{
		protected static byte[] emptySumBytes = new byte[] { 0, 0 };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort GetChecksum(byte[] buffer)
			=> GetChecksum((Span<byte>)buffer);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort GetChecksum(Span<byte> buffer)
		{
			emptySumBytes.CopyTo(buffer[3..]);

#if NET5_0_OR_GREATER
			return (ushort)(SHA256.HashData(buffer.ToArray()).Sum(x => x) % ushort.MaxValue);
#endif
			using (var hasher = SHA256.Create())
			{
				return (ushort)(hasher.ComputeHash(buffer.ToArray()).Sum(x => x) % ushort.MaxValue);
			}
		}
		// 0 byte - head

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort ReadPID(Span<byte> buffer) => BitConverter.ToUInt16(buffer[1..]); // end offset = 3

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UDPChannelEnum ReadChannel(Span<byte> buffer) => (UDPChannelEnum)buffer[5]; // end offset = 6

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort ReadChecksum(Span<byte> buffer) => BitConverter.ToUInt16(buffer[3..]); // end offset = 10
	}
}
