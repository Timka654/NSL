using NSL.UDP.Enums;
using System;
using System.Runtime.CompilerServices;

namespace NSL.UDP.Packet
{
    internal class LPacket
	{
		public static byte[] LPHeadBytes = new byte[] { 0 };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadISLP(Span<byte> buffer) => (DgramHeadTypeEnum)buffer[0] == DgramHeadTypeEnum.LP; // end offset 1


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort ReadPacketLen(Span<byte> buffer) => BitConverter.ToUInt16(buffer[6..]); // end offset = 8

		// 8..10 - is Checksum

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Memory<byte> CreateHeader(byte[] pidBytes, byte channelBytes, ushort count)
		{
			var lpBuf = (Memory<byte>)new byte[8];

			LPHeadBytes
				.CopyTo(lpBuf);

			pidBytes
				.CopyTo(lpBuf[1..]);

			lpBuf.Span[5] = channelBytes;

			BitConverter.GetBytes(count)
				.CopyTo(lpBuf[6..]);

			var lpBufRes = lpBuf.ToArray();

			var cs = UDPPacket.GetChecksum(lpBufRes);

			BitConverter.GetBytes(cs)
				.CopyTo(lpBuf[3..]);

			return lpBuf;
		}
	}
}
