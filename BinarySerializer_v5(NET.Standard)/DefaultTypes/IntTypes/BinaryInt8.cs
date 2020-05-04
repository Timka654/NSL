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
    public partial class BinaryInt8 : IBasicType
    {
        public Type CompareType => typeof(byte);

        public BinaryInt8()
        { 
        
        }

#if NOT_UNITY

        public void GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var r = il.DeclareLocal(typeof(byte));

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
                prop.PropertySetter(il);
            }
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, bool listValue)
        {
            //BinaryStruct.WriteSizeChecker(il, buffer, offset, 1);
            var arr = currentStruct.TempBuildValues["tempBuffer"].Value;

            il.Ldloc(arr);
            il.Ldc_I4(0);
            il.Ldloc(value);
            if (!listValue)
                prop.PropertyGetter(il);
            il.Stelem(typeof(byte));

            //il.Ldloc(buffer);
            //il.Ldloc(offset);
            //il.Ldloc(arrSize);
            //il.Ldc_I4(0);
            //il.Ldelem(typeof(byte));
            //il.Stelem(typeof(byte));
            //il.Stloc(arr);

            il.ArraySetter(buffer, arr, 1);

            //BinaryStruct.WriteOffsetAppend(il, offset, 1);
        }
#endif
    }
}
