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
    public class BinaryUInt16 : IBasicType
    {
        public Type CompareType => typeof(ushort);

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        public BinaryUInt16()
        {
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes",new Type[] { typeof(ushort) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToUInt16", new Type[] { typeof(byte[]), typeof(int) });
                
        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var r = il.DeclareLocal(typeof(ushort));

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            if (listValue)
                il.Stloc(result);
            else
                il.Stloc(r);

            BinaryStruct.WriteOffsetAppend(il, offset, 2);
            if (!listValue)
            {
                il.Ldloc(result);
                il.Ldloc(r);
                il.Call(prop.Setter, isVirtual: true);
            }
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 2);
            var arr = il.DeclareLocal(typeof(byte[]));

            il.Ldloc(value);
            if (!listValue)
                il.Call(prop.Getter);
            il.Dup();
            il.Pop();
            il.Call(writeBitConverterMethodInfo);
            il.Stloc(arr);

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldloc(arr);
            il.Ldc_I4(0);
            il.Ldelem(typeof(byte));
            il.Stelem(typeof(byte));

            for (int i = 1; i < 2; i++)
            {
                il.Ldloc(buffer);
                il.Ldloc(offset);
                il.Ldc_I4(i);
                il.Add();
                il.Ldloc(arr);
                il.Ldc_I4(i);
                il.Ldelem(typeof(byte));
                il.Stelem(typeof(byte));
            }
            BinaryStruct.WriteOffsetAppend(il, offset, 2);
        }
    }
}
