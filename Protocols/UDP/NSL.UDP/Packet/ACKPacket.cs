using NSL.UDP.Enums;
using System;
using System.Runtime.CompilerServices;

namespace NSL.UDP.Packet
{
    internal class ACKPacket
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadISACK(Span<byte> buffer) => (DgramHeadTypeEnum)buffer[0] == DgramHeadTypeEnum.ACK; // end offset 1

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadPID(Span<byte> buffer) => BitConverter.ToUInt32(buffer[4..]); // end offset 8

    }
}
