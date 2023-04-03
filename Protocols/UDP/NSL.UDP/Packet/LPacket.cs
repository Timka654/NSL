using NSL.UDP.Enums;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace NSL.UDP.Packet
{
    internal class LPacket
	{
		public static byte LPHeadByte = (byte)DgramHeadTypeEnum.LP;

        public const byte LPLen = UDPPacket.BaseHeadLen + 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadISLP(Span<byte> buffer) => (DgramHeadTypeEnum)buffer[0] == DgramHeadTypeEnum.LP; // end offset 1

		// 1 - channel

		// 2..4 - is Checksum

		// 4..8 - pid

		// 8..10 - part count

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort ReadPacketLen(Span<byte> buffer) => BitConverter.ToUInt16(buffer[8..]); // end offset = 10


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Memory<byte> CreateHeader(byte[] pidBytes, byte channelByte, ushort count)
		{
			var lpBuf = (Memory<byte>)new byte[LPLen];

			lpBuf.Span[0] = LPHeadByte;

			lpBuf.Span[1] = channelByte;

			// 2..4 checksum

			pidBytes
				.CopyTo(lpBuf[4..]);


			BitConverter.GetBytes(count)
				.CopyTo(lpBuf[8..]);


			var lpBufRes = lpBuf.ToArray();

			var cs = UDPPacket.GetChecksum(lpBufRes);

			BitConverter.GetBytes(cs)
				.CopyTo(lpBuf[2..]);

			return lpBuf;
		}
	}
}
