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
    public class BinaryInt32 : IBasicType
    {
        public string SizeProperty { get; set; }

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        public BinaryInt32()
        {
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes",new Type[] { typeof(int) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToInt32", new Type[] { typeof(byte[]), typeof(int) });
                
        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset)
        {
            var r = il.DeclareLocal(typeof(int));

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            il.Stloc(r);

            il.Ldloc(result);
            il.Ldloc(r);
            il.Call(prop.Setter, isVirtual: true);
            BinaryStruct.WriteOffsetAppend(il, offset, 4);
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset)
        {
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 4);
            var arr = il.DeclareLocal(typeof(byte[]));

            il.Ldloc(value);
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

            for (int i = 1; i < 4; i++)
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
            BinaryStruct.WriteOffsetAppend(il, offset, 4);
        }
    }
}
