using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryString16 : IBasicType
    {
        public Type CompareType => typeof(string);

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private MethodInfo codingMethodInfo;

        public BinaryString16()
        {
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes", new Type[] { typeof(short) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToInt16", new Type[] { typeof(byte[]), typeof(int) });
            codingMethodInfo = typeof(BinaryStruct).GetProperty("Coding").GetMethod;
        }

        public void GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var exitLabel = il.DefineLabel("exit");
            BinaryStruct.ReadObjectNull(il, exitLabel, buffer, offset, typeSize);
            var len = il.DeclareLocal(typeof(short));

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            il.Stloc(len);

            BinaryStruct.WriteOffsetAppend(il, offset, 2);

            il.Ldloc(len);
            il.Ldc_I4(0);
            il.Ceq();
            il.Brtrue(exitLabel);

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

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, bool listValue)
        {
            //BinaryStruct.WriteSizeChecker(il, buffer, offset, 3);

            var arr = currentStruct.TempBuildValues["tempBuffer"].Value;
            var arrSize = currentStruct.TempBuildValues["tempLenghtBuffer"].Value;

            var temp = il.DeclareLocal(typeof(string));

            var exitLabel = il.DefineLabel("exit");


            il.Ldloc(value);

            if (!listValue)
                il.Call(prop.Getter, isVirtual: prop.Getter.IsVirtual);
            il.Stloc(temp);

            il.Ldloc(temp);
            BinaryStruct.WriteObjectNull(currentStruct, il, exitLabel, buffer, typeSize);

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



            il.ArraySetter(buffer, arrSize, 2);


            //il.Ldloc(buffer);
            //il.Ldloc(offset);
            //il.Ldloc(arrSize);
            //il.Ldc_I4(0);
            //il.Ldelem(typeof(byte));
            //il.Stelem(typeof(byte));

            //for (int i = 1; i < 2; i++)
            //{
            //    il.Ldloc(buffer);
            //    il.Ldloc(offset);
            //    il.Ldc_I4(i);
            //    il.Add();
            //    il.Ldloc(arrSize);
            //    il.Ldc_I4(i);
            //    il.Ldelem(typeof(byte));
            //    il.Stelem(typeof(byte));
            //}

            //BinaryStruct.WriteOffsetAppend(il, offset, 2);

            il.Ldloc(typeSize);
            il.Ldc_I4(0);
            il.Ceq();
            il.Brtrue(exitLabel);
            il.ArraySetter(buffer, arr, typeSize);

            //BinaryStruct.WriteSizeChecker(il, buffer, offset, typeSize);

            //var ivar = il.DeclareLocal(typeof(int));
            //var point = il.DefineLabel("for_label");

            //il.Ldc_I4(0);
            //il.Stloc(ivar);

            //il.MarkLabel(point);

            ////body

            //il.Ldloc(buffer);
            //il.Ldloc(ivar);
            //il.Ldloc(offset);
            //il.Add();
            //il.Ldloc(arr);
            //il.Ldloc(ivar);

            //il.Ldelem(typeof(byte));
            //il.Stelem(typeof(byte));

            ////end body

            //il.Ldc_I4(1);
            //il.Ldloc(ivar);
            //il.Add();
            //il.Stloc(ivar);

            //il.Ldloc(ivar);
            //il.Ldloc(typeSize);

            //il.Clt(false);
            //il.Brtrue(point);

            //BinaryStruct.WriteOffsetAppend(il, offset, typeSize);
            il.MarkLabel(exitLabel);
        }
    }
}
