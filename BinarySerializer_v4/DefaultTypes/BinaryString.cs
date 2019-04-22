using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryString : BasicType
    {
        public new static int GetSize(object value)
        {
            return Encoding.UTF8.GetByteCount((string)value);
        }

        private int _size;

        public override bool FixedSize => false;

        public override int Size {
            get => _size;
            set => _size = value;
        }

        public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            if (value == null)
                return;
            byte[] s = this.Serializer.TextCoding.GetBytes((string)value);
            for (int i = 0; i < Size && i < s.Length; i++)
            {
                buffer[offset + i] = s[i];
            }
        }

        public override object GetValue(ref byte[] buffer, int offset)
        {
            if (Size == 0)
                return "";
            return this.Serializer.TextCoding.GetString(buffer, offset, Size).Replace("\0", "");
        }
    }
}
