using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializerTest
{
    public class TypeArrayTestClass
    {
        [Binary(typeof(TypeTestClass),ArraySize = 10)]
        public TypeTestClass[] arr0 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int len { get; set; }

        [Binary(typeof(TypeTestClass), ArraySizeName = "len")]
        public TypeTestClass[] arr1 { get; set; }

    }
}
