﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryList16<T> : IBasicType
    {
        public Type CompareType => typeof(IList);

        public string SizeProperty { get; set; }

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private MethodInfo codingMethodInfo;

        public BinaryList16()
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
            var list = il.DeclareLocal(prop.Type);

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            il.Stloc(len);

            BinaryStruct.WriteOffsetAppend(il, offset, 2);

            il.Newobj(BinaryStruct.GetConstructor(prop.Type, null));

            il.Stloc(list);
            il.Ldloc(result);
            il.Ldloc(list);
            prop.PropertySetter(il);

            il.Ldloc(len);
            il.Ldc_I4(0);
            il.Ceq();
            il.Brtrue(exitLabel);

            var type = prop.Type.GetGenericArguments()[0];

            var ivar = il.DeclareLocal(typeof(int));
            var point = il.DefineLabel("for_label");

            il.Ldc_I4(0);
            il.Stloc(ivar);

            il.MarkLabel(point);

            //body

            var tempVar = il.DeclareLocal(type);

            if (typeof(IBasicType).IsAssignableFrom(prop.BinaryAttr.Type.GetGenericArguments()[0]))
            {
                IBasicType t = (IBasicType)Activator.CreateInstance(prop.BinaryAttr.Type.GetGenericArguments()[0]);
                t.GetReadILCode(prop, currentStruct, il, binaryStruct,buffer, tempVar, typeSize, offset,true);
                il.Ldloc(list);
                il.Ldloc(tempVar);
                il.Call(prop.Type.GetMethod("Add"), isVirtual: true);
            }
            else
            {
                var constr = BinaryStruct.GetConstructor(type, null);
                if (constr == null)
                    throw new Exception($"Type {type} not have constructor with not parameters");

                il.Newobj(constr);
                il.Stloc(tempVar);

                BinaryStruct.CompileReader(currentStruct.CurrentStorage.GetTypeInfo(type, currentStruct.Scheme), il, binaryStruct, buffer, offset, tempVar, typeSize);

                il.Ldloc(list);
                il.Ldloc(tempVar);
                il.Call(prop.Type.GetMethod("Add"), isVirtual:true);
            }

            //end body

            il.Ldc_I4(1);
            il.Ldloc(ivar);
            il.Add();
            il.Stloc(ivar);

            il.Ldloc(ivar);
            il.Ldloc(len);

            il.Clt(false);
            il.Brtrue(point);

            il.MarkLabel(exitLabel);
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, bool listValue)
        {
            var arr = il.DeclareLocal(prop.Type);

            il.Ldloc(value);
            prop.PropertyGetter(il);
            il.Stloc(arr);

            var exitLabel = il.DefineLabel("exit");

            //BinaryStruct.WriteSizeChecker(il, buffer, offset, 3);
            BinaryStruct.WriteObjectNull(currentStruct, il, exitLabel, arr, buffer, typeSize);

            var arrSize = currentStruct.TempBuildValues["tempLenghtBuffer"].Value;
            var len = il.DeclareLocal(typeof(short));
            il.Ldloc(value);
            prop.PropertyGetter(il);
            il.Call(typeof(ICollection).GetProperty("Count").GetMethod);
            il.Stloc(len);

            il.Ldloc(len);
            il.Call(writeBitConverterMethodInfo);
            il.Stloc(arrSize);

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

            il.ArraySetter(buffer, arrSize, 2);
            //BinaryStruct.WriteOffsetAppend(il, offset, 2);


            il.Ldloc(len);
            il.Ldc_I4(0);
            il.Ceq();
            il.Brtrue(exitLabel);

            var type = prop.Type.GetGenericArguments()[0];

            var ivar = il.DeclareLocal(typeof(int));
            var currentValue = il.DeclareLocal(type);
            var point = il.DefineLabel("for_label");

            il.Ldc_I4(0);
            il.Stloc(ivar);

            il.MarkLabel(point);

            //body


            il.Ldloc(arr);
            il.Ldloc(ivar);
            il.Call(prop.Type.GetMethod("get_Item"), isVirtual: true);
            il.Stloc(currentValue);

            if (typeof(IBasicType).IsAssignableFrom(prop.BinaryAttr.Type.GetGenericArguments()[0]))
            {
                IBasicType t = (IBasicType)Activator.CreateInstance(prop.BinaryAttr.Type.GetGenericArguments()[0]);
                t.GetWriteILCode(prop, currentStruct, il, binaryStruct, currentValue, typeSize, buffer,true);
            }
            else
            {
                BinaryStruct.CompileWriter(currentStruct.CurrentStorage.GetTypeInfo(type,currentStruct.Scheme), il, binaryStruct, currentValue, buffer, typeSize);
            }
            //end body

            il.Ldc_I4(1);
            il.Ldloc(ivar);
            il.Add();
            il.Stloc(ivar);

            il.Ldloc(ivar);
            il.Ldloc(len);

            il.Clt(false);
            il.Brtrue(point);

            il.MarkLabel(exitLabel);
        }
    }
}
