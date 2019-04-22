using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryBool : BasicType
    {
        public new static int GetSize(object value)
        {
            return 1;
        }
        public override bool FixedSize => true;

        public override int Size { get => 1; set { } }

        public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            buffer[offset] = (byte)(((bool)value) ? 1 : 0);
        }

        public override unsafe object GetValue(ref byte[] buffer, int offset)
        {
            return buffer[offset] == 1;
        }
    }
}
