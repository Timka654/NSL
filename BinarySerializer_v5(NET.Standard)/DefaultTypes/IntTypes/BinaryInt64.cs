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
    public partial class BinaryInt64 : IBasicType
    {
        public Type CompareType => typeof(long);

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        public BinaryInt64()
        {
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes",new Type[] { typeof(long) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToInt64", new Type[] { typeof(byte[]), typeof(int) });

        }

#if NOT_UNITY

        public void GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var r = il.DeclareLocal(typeof(long));

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            if (listValue)
                il.Stloc(result);
            else
                il.Stloc(r);

            BinaryStruct.WriteOffsetAppend(il, offset, 8);
            if (!listValue)
            {
                il.Ldloc(result);
                il.Ldloc(r);
                prop.PropertySetter(il);
            }
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, bool listValue)
        {
            //BinaryStruct.WriteSizeChecker(il, buffer, offset, 8);
            var arr = currentStruct.TempBuildValues["tempBuffer"].Value;

            il.Ldloc(value);
            if (!listValue)
                prop.PropertyGetter(il);
            il.Dup();
            il.Pop();
            il.Call(writeBitConverterMethodInfo);
            il.Stloc(arr);

            il.ArraySetter(buffer, arr, 8);
            //BinaryStruct.WriteOffsetAppend(il, offset, 8);
        }

#endif

    }
}
