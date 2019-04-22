using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryUInt16 : BasicType
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
                *((ushort*)(b + offset)) = (ushort)value;
        }

        public override unsafe object GetValue(ref byte[] buffer, int offset)
        {
            fixed (byte* pbyte = &buffer[offset])
                return (ushort)((*pbyte) | (*(pbyte + 1) << 8));
        }
    }
}
