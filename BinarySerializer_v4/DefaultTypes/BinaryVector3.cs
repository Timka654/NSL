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
    public class BinaryVector3 : BasicType
    {
        public new static int GetSize(object value)
        {
            return 12;
        }
        public override bool FixedSize => true;

        public override int Size { get => 12; set { } }

        public override unsafe void GetBytes(ref byte[] buffer, int offset, object value)
        {
            Vector3 v = (Vector3)value;

            fixed (byte* b = buffer)
            {
                *((int*)(b + offset)) = *(int*)&v.X;
                *((int*)(b + offset + 4)) = *(int*)&v.Y;
                *((int*)(b + offset + 4)) = *(int*)&v.Z;
            }
        }

        public override unsafe object GetValue(ref byte[] buffer, int offset)
        {
            fixed (byte* pbyte = &buffer[offset])
            {
                int x = ((*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24));
                int y = (((*pbyte + 4) << 32) | (*(pbyte + 5) << 40) | (*(pbyte + 6) << 48) | (*(pbyte + 7) << 56));
                int z = (((*pbyte + 8) << 64) | (*(pbyte + 9) << 72) | (*(pbyte + 10) << 80) | (*(pbyte + 11) << 88));

                return new Vector3(*(float*)&x, *(float*)&y, *(float*)&z);
            }
        }
    }
}
