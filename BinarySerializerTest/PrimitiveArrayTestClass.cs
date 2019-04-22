using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializerTest
{
    public class PrimitiveArrayTestClass
    {
        [Binary(typeof(BinaryInt32), ArraySize = 5)]
        public int[] ia0 { get; set; }

        [Binary(typeof(BinaryInt32), ArraySize = 10)]
        public int[] ia1 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int len { get; set; }

        [Binary(typeof(BinaryInt32), ArraySizeName = "len")]
        public int[] ia2 { get; set; }
    }
}
