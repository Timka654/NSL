using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryString32 : BasicType
    {
        public new static int GetSize(object value)
        {
            return Encoding.UTF8.GetByteCount((string)value);
        }

        private int _size;

        public override bool FixedSize => true;

        public override int Size
        {
            get => _size;
            set => _size = value;
        }

        public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            if (value == null)
                return;
            byte[] s = this.Serializer.TextCoding.GetBytes((string)value);

            while (buffer.Length - offset < s.Length + 4)
            {
                Array.Resize(ref buffer, buffer.Length * 2);
            }

            var size = BitConverter.GetBytes((int)s.Length);

            buffer[offset] = size[0];
            buffer[offset + 1] = size[1];
            buffer[offset + 2] = size[2];
            buffer[offset + 3] = size[3];

            for (int i = 0; i < s.Length; i++)
            {
                buffer[offset + i + 4] = s[i];
            }

            Size = s.Length + 4;
        }

        public override object GetValue(ref byte[] buffer, int offset)
        {
            Size = BitConverter.ToInt32(buffer, offset) + 4;
            if (Size == 4)
            { return ""; }
            return this.Serializer.TextCoding.GetString(buffer, offset + 4, Size - 4).Replace("\0", "");
        }
    }
}
