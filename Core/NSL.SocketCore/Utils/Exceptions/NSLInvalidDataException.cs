using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.SocketCore.Utils.Exceptions
{
    public class NSLInvalidDataException : Exception
    {
        public NSLInvalidDataException(int len, int offset, Span<byte> buffer, InputPacketBuffer packet)
        {
            Len = len;
            Offset = offset;
            Buffer = buffer.ToArray();
            Packet = packet;
        }

        public int Len { get; }
        public int Offset { get; }
        public byte[] Buffer { get; }
        public InputPacketBuffer Packet { get; }
    }
}
