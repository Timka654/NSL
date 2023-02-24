using NSL.UDP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Packet
{
	internal class CompPacket
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadISComp(Memory<byte> buffer) => (DgramHeadTypeEnum)buffer.Span[0] == DgramHeadTypeEnum.Comp; // end offset 1
	}
}
