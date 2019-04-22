using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryUInt64 : BasicType
    {
        public new static int GetSize(object value)
        {
            return 8;
        }
        public override bool FixedSize => true;

        public override int Size { get => 8; set { } }

        public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            fixed (byte* b = buffer)
                *((ulong*)(b + offset)) = (ulong)value;
        }

        public override unsafe object GetValue(ref byte[] buffer, int offset)
        {
            fixed (byte* pbyte = &buffer[offset])
                return (ulong)((*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24) | (*(pbyte + 3) << 32) | (*(pbyte + 3) << 40) | (*(pbyte + 3) << 48) | (*(pbyte + 3) << 56));
        }
    }
}
