using NSL.UDP.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Packet
{
	internal class DataPacket
	{
		public static byte[] DataHeadBytes = new byte[] { 1 };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<Memory<byte>> CreateParts(byte[] pidBytes, byte channelByte, byte[] data, IUDPOptions options)
		{
			List<Memory<byte>> result = new List<Memory<byte>>();

			ushort h = default;

			for (int i = 0; i < data.Length; i += options.SendFragmentSize)
			{
				var dest = i + options.SendFragmentSize > data.Length ? data[i..] : data[i..(i + options.SendFragmentSize)];

				Memory<byte> pbuf = new byte[10 + dest.Length];

				DataPacket.DataHeadBytes
					.CopyTo(pbuf);

				pidBytes
					.CopyTo(pbuf[1..]);

				pbuf.Span[5] = channelByte;

				BitConverter.GetBytes(h++)
					.CopyTo(pbuf[6..]);

				dest
					.CopyTo(pbuf[8..]);

				var pbufResult = pbuf.ToArray();

				BitConverter.GetBytes(UDPPacket.GetChecksum(pbufResult))
					.CopyTo(pbuf[3..]);

				result.Add(pbuf);
			}

			return result;
		}
	}
}
