using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryFloat64 : BasicType
    {
        public new static int GetSize(object value)
        {
            return 8;
        }
        public override bool FixedSize => true;

        public override int Size { get => 8; set { } }

        public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            var val = (double)value;
            fixed (byte* b = buffer)
                *((long*)(b + offset)) = *(long*)&val;
        }

        public override unsafe object GetValue(ref byte[] buffer, int offset)
        {
            fixed (byte* pbyte = &buffer[offset])
            {
                long val = ((uint)((*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24)) | (*(pbyte + 4)) | ((long)(*(pbyte + 5) << 8) | (*(pbyte + 6) << 16) | (*(pbyte + 7) << 24)) << 32);
                return *(double*)&val;
            }
        }
    }
}
