using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryDictionary : BasicType
    {
        public new static int GetSize(object value)
        {
            return 1;
        }
        public override bool FixedSize => false;

        public override int Size { get => 1; set { } }

        public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            buffer[offset] = (byte)value;
        }

        public override unsafe object GetValue(ref byte[] buffer, int offset)
        {
            return buffer[offset];
        }
    }
}
