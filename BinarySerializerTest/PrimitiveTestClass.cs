using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializerTest
{
    public class PrimitiveTestClass
    {
        [Binary(typeof(BinaryInt16))]
        public short i0 { get; set; }

        [Binary(typeof(BinaryUInt16))]
        public ushort i1 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int i2 { get; set; }

        [Binary(typeof(BinaryUInt32))]
        public uint i3 { get; set; }

        [Binary(typeof(BinaryInt64))]
        public long i4 { get; set; }

        [Binary(typeof(BinaryUInt64))]
        public ulong i5 { get; set; }

        [Binary(typeof(BinaryFloat32))]
        public float f0 { get; set; }

        [Binary(typeof(BinaryFloat64))]
        public double f1 { get; set; }

        [Binary(typeof(BinaryString), TypeSize = 10)]
        public string s0 { get; set; }

        [Binary(typeof(BinaryString), TypeSize = 55)]
        public string s1 { get; set; }
        
        [Binary(typeof(BinaryInt32))]
        public int len { get; set; }

        [Binary(typeof(BinaryString),TypeSizeName = "len")]
        public string s2 { get; set; }
    }
}
