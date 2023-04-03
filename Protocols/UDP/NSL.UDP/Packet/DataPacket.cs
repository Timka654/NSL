using NSL.UDP.Enums;
using NSL.UDP.Interface;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NSL.UDP.Packet
{
    internal class DataPacket
	{
		public static byte DataHeadByte = (byte)DgramHeadTypeEnum.Data;

		public const byte DataHeadLen = UDPPacket.BaseHeadLen + 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadPOffset(Span<byte> buffer) => BitConverter.ToUInt16(buffer[8..]); // end offset = 10

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<Memory<byte>> CreateParts(byte[] pidBytes, byte channelByte, byte[] data, IUDPOptions options)
		{
			List<Memory<byte>> result = new List<Memory<byte>>();

			ushort h = default;

			for (int offset = 0; offset < data.Length; offset += options.SendFragmentSize)
			{
				var dest = offset + options.SendFragmentSize > data.Length ? data[offset..] : data[offset..(offset + options.SendFragmentSize)];

				Memory<byte> pbuf = new byte[DataHeadLen + dest.Length];

				pbuf.Span[0] = DataHeadByte;

				pbuf.Span[1] = channelByte;

				//2..4 - checksum

                pidBytes
					.CopyTo(pbuf[4..]);

				BitConverter.GetBytes(h++)
					.CopyTo(pbuf[8..]);

				dest
					.CopyTo(pbuf[10..]);


				var pbufResult = pbuf.ToArray();

				BitConverter.GetBytes(UDPPacket.GetChecksum(pbufResult))
					.CopyTo(pbuf[2..]);

				result.Add(pbuf);
			}

			return result;
		}
	}
}
