using NSL.UDP.Enums;
using System;
using System.Runtime.CompilerServices;

namespace NSL.UDP.Packet
{
    internal class CompPacket
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadISComp(Span<byte> buffer) => (DgramHeadTypeEnum)buffer[0] == DgramHeadTypeEnum.Comp; // end offset 1
	}
}
