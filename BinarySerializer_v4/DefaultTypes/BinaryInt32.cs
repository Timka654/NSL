using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryInt32 : BasicType
    {
        public new static int GetSize(object value)
        {
            return 4;
        }
        public override bool FixedSize => true;

        public override int Size { get => 4; set { } }

public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            fixed (byte* b = buffer)
                *((int*)(b + offset)) = (int)value;
        }

        public override unsafe object GetValue(ref byte[] buffer, int offset)
        {
            fixed (byte* pbyte = &buffer[offset])
                return (*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24);
        }
    }
}
