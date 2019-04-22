using System;
using System.Collections.Generic;
#if UNITY_STANDALONE
using UnityEngine;
#else
using System.Numerics;
#endif
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryVector2 : BasicType
    {
        public new static int GetSize(object value)
        {
            return 8;
        }
        public override bool FixedSize => true;

        public override int Size { get => 8; set { } }

        public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            Vector2 v = (Vector2)value;

            fixed (byte* b = buffer)
            {
                *((int*)(b + offset)) = *(int*)&v.X;
                *((int*)(b + offset + 4)) = *(int*)&v.Y;
            }
        }

        public override unsafe object GetValue(ref byte[] buffer, int offset)
        {
            fixed (byte* pbyte = &buffer[offset])
            {
                int x = ((*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24));
                int y = (((*pbyte + 4) << 32) | (*(pbyte + 5) << 40) | (*(pbyte + 6) << 48) | (*(pbyte + 7) << 56));

                return new Vector2(*(float*)&x, *(float*)&y);
            }
        }
    }
}
