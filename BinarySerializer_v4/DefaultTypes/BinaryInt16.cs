using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryInt16 : BasicType
    {
        public new static int GetSize(object value)
        {
            return 2;
        }
        public override bool FixedSize => true;

        public override int Size { get => 2; set { } }

        public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            fixed (byte* b = buffer)
                *((short*)(b + offset)) = (short)value;
        }

        public override unsafe object GetValue(ref byte[] buffer, int offset)
        {
            fixed (byte* pbyte = &buffer[offset])
                return (short)((*pbyte) | (*(pbyte + 1) << 8));
        }
    }
}
