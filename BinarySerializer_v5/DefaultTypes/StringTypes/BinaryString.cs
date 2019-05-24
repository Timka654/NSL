using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryString : IBasicType
    {
        public Type CompareType => null;

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private MethodInfo codingMethodInfo;

        public BinaryString()
        {
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes", new Type[] { typeof(int) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToInt32", new Type[] { typeof(byte[]), typeof(int) });
            codingMethodInfo = typeof(BinaryStruct).GetProperty("Coding").GetMethod;
        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var len = il.DeclareLocal(typeof(short));

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            il.Stloc(len);

            BinaryStruct.WriteOffsetAppend(il, offset, 2);

            if (!listValue)
            {
                il.Ldloc(result);
                il.Ldarg(1);
            }
            else
            {
                il.Ldloc(binaryStruct);
            }
            il.Call(codingMethodInfo);

            il.Castclass(typeof(UTF8Encoding));

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldloc(len);

            il.Call(currentStruct.Coding.GetType().GetMethod("GetString", new Type[] { typeof(byte[]), typeof(int), typeof(int) }), isVirtual: true);

            if (!listValue)
                il.Call(prop.Setter);
            else
                il.Stloc(result);

            BinaryStruct.WriteOffsetAppend(il, offset, len);
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            var exitLabel = il.DefineLabel("exit");
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 4);

            var arr = il.DeclareLocal(typeof(byte[]));

            il.Ldarg(1);
            il.Call(codingMethodInfo);
            il.Ldloc(value);

            if (!listValue)
                il.Call(prop.Getter);

            il.Dup();
            BinaryStruct.WriteObjectNull(il, exitLabel, buffer, offset, typeSize);

            il.Call(currentStruct.Coding.GetType().GetMethod("GetBytes", new Type[] { typeof(string) }));
            il.Stloc(arr);


            if (prop.PropertyInfo != null)
            {
                il.Ldloc(value);
                il.Call(prop.TypeSizeProperty.Getter);
                il.Stloc(typeSize);
            }
            else
            {
                il.Ldc_I4(prop.TypeSize);
                il.Stloc(typeSize);
            }

            BinaryStruct.WriteSizeChecker(il, buffer, offset, typeSize);

            var ivar = il.DeclareLocal(typeof(int));
            var point = il.DefineLabel("for_label");

            il.Ldc_I4(0);
            il.Stloc(ivar);

            il.MarkLabel(point);

            //body

            il.Ldloc(buffer);
            il.Ldloc(ivar);
            il.Ldloc(offset);
            il.Add();
            il.Ldloc(arr);
            il.Ldloc(ivar);

            il.Ldelem(typeof(byte));
            il.Stelem(typeof(byte));

            //end body

            il.Ldc_I4(1);
            il.Ldloc(ivar);
            il.Add();
            il.Stloc(ivar);

            il.Ldloc(ivar);
            il.Ldloc(typeSize);

            il.Clt(false);
            il.Brtrue(point);

            BinaryStruct.WriteOffsetAppend(il, offset, typeSize);

        }
    }
}
