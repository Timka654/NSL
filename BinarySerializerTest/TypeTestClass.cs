using BinarySerializer;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializerTest
{
    public class TypeTestClass
    {
        [Binary(typeof(PrimitiveTestClass))]
        public PrimitiveTestClass c0 { get; set; }

        [Binary(typeof(PrimitiveTestClass))]
        public PrimitiveTestClass c1 { get; set; }

        [Binary(typeof(PrimitiveTestClass))]
        public PrimitiveTestClass c2 { get; set; }

        [Binary(typeof(PrimitiveTestClass))]
        public PrimitiveTestClass c3 { get; set; }
    }
}
