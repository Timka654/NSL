using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;
using GrEmit;
using GrEmit.Utils;

namespace BinarySerializer.DefaultTypes
{
    public class BinarySInt8 : IBasicType
    {
        public Type CompareType => typeof(sbyte);

        public BinarySInt8()
        {
                
        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var r = il.DeclareLocal(typeof(sbyte));

            il.Ldloc(buffer);
            il.Ldloc(offset);

            il.Ldelem(typeof(byte));
            if (listValue)
                il.Stloc(result);
            else
                il.Stloc(r);

            BinaryStruct.WriteOffsetAppend(il, offset, 1);
            if (!listValue)
            {
                il.Ldloc(result);
                il.Ldloc(r);
                il.Call(prop.Setter, isVirtual: true);
            }
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 1);

            il.Ldloc(buffer);
            il.Ldloc(offset);

            il.Ldloc(value);
            if (!listValue)
                il.Call(prop.Getter);

            il.Stelem(typeof(byte));

            BinaryStruct.WriteOffsetAppend(il, offset, 1);
        }
    }
}
