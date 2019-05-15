using BinarySerializer.DefaultTypes;
using GrEmit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BinarySerializer
{
    public class BinarySerializer
    {
        [Binary(typeof(BinaryInt32))]
        public int v1 { get; set; } = 99;

        public void TestType()
        {
            var r = TypeStorage.Instance.GetTypeInfo(typeof(BinarySerializer), "").WriteMethod(this);

        }
    }
}
