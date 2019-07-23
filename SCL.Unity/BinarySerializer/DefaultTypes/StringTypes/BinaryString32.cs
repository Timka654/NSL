using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryString32 : IBasicType
    {
        public Type CompareType => null;

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private MethodInfo codingMethodInfo;

        public BinaryString32()
        {
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes", new Type[] { typeof(int) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToInt32", new Type[] { typeof(byte[]), typeof(int) });
            codingMethodInfo = typeof(BinaryStruct).GetProperty("Coding").GetMethod;
        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var exitLabel = il.DefineLabel("exit");
            BinaryStruct.ReadObjectNull(il, exitLabel, buffer, offset, typeSize);
            var len = il.DeclareLocal(typeof(int));

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            il.Stloc(len);

            BinaryStruct.WriteOffsetAppend(il, offset, 4);

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
            il.MarkLabel(exitLabel);
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 5);

            var arr = il.DeclareLocal(typeof(byte[]));
            var arrSize = il.DeclareLocal(typeof(byte[]));
            var temp = il.DeclareLocal(typeof(string));
            var exitLabel = il.DefineLabel("exit");


            il.Ldloc(value);

            if (!listValue)
                il.Call(prop.Getter);
            il.Stloc(temp);

            il.Ldloc(temp);
            BinaryStruct.WriteObjectNull(il, exitLabel, buffer, offset, typeSize);

            il.Ldloc(temp);
            il.Call(typeof(string).GetProperty("Length").GetMethod);
            il.Stloc(typeSize);

            il.Ldarg(1);
            il.Call(codingMethodInfo);

            il.Ldloc(temp);

            il.Call(currentStruct.Coding.GetType().GetMethod("GetBytes", new Type[] { typeof(string) }));
            il.Stloc(arr);

            il.Ldloc(arr);
            il.Call(typeof(byte[]).GetProperty("Length").GetMethod);
            il.Stloc(typeSize);

            il.Ldloc(typeSize);
            il.Call(writeBitConverterMethodInfo);
            il.Stloc(arrSize);

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldloc(arrSize);
            il.Ldc_I4(0);
            il.Ldelem(typeof(byte));
            il.Stelem(typeof(byte));

            for (int i = 1; i < 4; i++)
            {
                il.Ldloc(buffer);
                il.Ldloc(offset);
                il.Ldc_I4(i);
                il.Add();
                il.Ldloc(arrSize);
                il.Ldc_I4(i);
                il.Ldelem(typeof(byte));
                il.Stelem(typeof(byte));
            }

            BinaryStruct.WriteOffsetAppend(il, offset, 4);

            il.Ldloc(typeSize);
            il.Ldc_I4(0);
            il.Ceq();
            il.Brtrue(exitLabel);

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

            il.MarkLabel(exitLabel);
        }
    }
}
