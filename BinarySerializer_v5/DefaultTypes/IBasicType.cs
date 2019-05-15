using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public interface IBasicType
    {
        int TypeSize { get; set; }

        string SizeProperty { get; set; }

        void GetReadILCode(PropertyData prop, Type currentType, GroboIL generator);

        void GetWriteILCode(PropertyData prop, Type currentType, GroboIL generator);
    }
}
