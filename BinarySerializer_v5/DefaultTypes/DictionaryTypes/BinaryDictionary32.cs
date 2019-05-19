﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryDictionary32<TKey,TValue> : IBasicType
    {
        public string SizeProperty { get; set; }

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private MethodInfo codingMethodInfo;

        public BinaryDictionary32()
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
            var list = il.DeclareLocal(prop.PropertyInfo.PropertyType);
            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            il.Stloc(len);

            BinaryStruct.WriteOffsetAppend(il, offset, 4);

            il.Newobj(prop.PropertyInfo.PropertyType.GetConstructor(new Type[] { }));

            il.Stloc(list);
            il.Ldloc(result);
            il.Ldloc(list);
            il.Call(prop.Setter, isVirtual: true);



            il.Ldloc(len);
            il.Ldc_I4(0);
            il.Ceq();
            il.Brtrue(exitLabel);

            var typeKey = prop.PropertyInfo.PropertyType.GetGenericArguments()[0];
            var typeValue = prop.PropertyInfo.PropertyType.GetGenericArguments()[1];

            var ivar = il.DeclareLocal(typeof(int));
            var currentItemKey = il.DeclareLocal(typeKey);
            var currentItemValue = il.DeclareLocal(typeValue);

            var point = il.DefineLabel("for_label");

            il.Ldc_I4(0);
            il.Stloc(ivar);

            il.MarkLabel(point);

            //body
            

            //key
            if (typeof(IBasicType).IsAssignableFrom(prop.BinaryAttr.Type.GetGenericArguments()[0]))
            {
                IBasicType t = (IBasicType)Activator.CreateInstance(prop.BinaryAttr.Type.GetGenericArguments()[0]);
                t.GetReadILCode(prop, currentStruct, il, binaryStruct, buffer, currentItemKey, typeSize, offset, true);
            }
            else
            {
                var constr = typeKey.GetConstructor(new Type[] { });
                if (constr == null)
                    throw new Exception($"Type {typeKey} not have constructor with not parameters");

                il.Newobj(constr);
                il.Stloc(currentItemKey);

                BinaryStruct.CompileReader(currentStruct.CurrentStorage.GetTypeInfo(typeKey, currentStruct.Scheme), il, binaryStruct, buffer, offset, currentItemKey, typeSize);

            }

            //value
            if (typeof(IBasicType).IsAssignableFrom(prop.BinaryAttr.Type.GetGenericArguments()[1]))
            {
                IBasicType t = (IBasicType)Activator.CreateInstance(prop.BinaryAttr.Type.GetGenericArguments()[1]);
                t.GetReadILCode(prop, currentStruct, il, binaryStruct, buffer, currentItemValue, typeSize, offset, true);
            }
            else
            {
                var constr = typeValue.GetConstructor(new Type[] { });
                if (constr == null)
                    throw new Exception($"Type {typeValue} not have constructor with not parameters");

                il.Newobj(constr);
                il.Stloc(currentItemValue);

                BinaryStruct.CompileReader(currentStruct.CurrentStorage.GetTypeInfo(typeValue, currentStruct.Scheme), il, binaryStruct, buffer, offset, currentItemValue, typeSize);
            }

            il.Ldloc(list);
            il.Ldloc(currentItemKey);
            il.Ldloc(currentItemValue);
            il.Call(prop.PropertyInfo.PropertyType.GetMethod("Add"), isVirtual: true);

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

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            var arr = il.DeclareLocal(prop.PropertyInfo.PropertyType);

            il.Ldloc(value);
            il.Call(prop.Getter);
            il.Stloc(arr);

            var exitLabel = il.DefineLabel("exit");
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 5);
            BinaryStruct.WriteObjectNull(il, exitLabel, arr, buffer, offset, typeSize);

            var arrSize = il.DeclareLocal(typeof(byte[]));
            var len = il.DeclareLocal(typeof(int));
            il.Ldloc(value);
            il.Call(prop.Getter);
            il.Call(typeof(ICollection).GetProperty("Count").GetMethod);
            il.Stloc(len);

            il.Ldloc(len);
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


            il.Ldloc(len);
            il.Ldc_I4(0);
            il.Ceq();
            il.Brtrue(exitLabel);

            var typeKey = prop.PropertyInfo.PropertyType.GetGenericArguments()[0];
            var typeValue = prop.PropertyInfo.PropertyType.GetGenericArguments()[1];

            var ivar = il.DeclareLocal(typeof(int));
            var currentItemKey = il.DeclareLocal(typeKey);
            var currentItemValue = il.DeclareLocal(typeValue);

            var point = il.DefineLabel("for_label");

            il.Ldc_I4(0);
            il.Stloc(ivar);

            var enumeratorMethod = prop.PropertyInfo.PropertyType.GetMethod("GetEnumerator");

            var enumerator = il.DeclareLocal(enumeratorMethod.ReturnType);

            var moveNext = enumerator.Type.GetMethod("MoveNext");
            var getCurrent = enumerator.Type.GetMethod("get_Current");

            var temp = il.DeclareLocal(getCurrent.ReturnType);
            var exist = il.DeclareLocal(typeof(bool));

            il.Ldloc(arr);
            il.Call(enumeratorMethod, isVirtual: true);
            il.Stloc(enumerator);

            var keyGetter = getCurrent.ReturnType.GetMethod("get_Key");
            var valueGetter = getCurrent.ReturnType.GetMethod("get_Value");

            il.MarkLabel(point);

            //body

            il.Ldloca(enumerator);
            il.Call(moveNext, enumerator.Type);
            il.Stloc(exist);
            
            il.Ldloca(enumerator);
            //il.Calli(CallingConventions.Any, typeof(KeyValuePair<int, int>),new Type[] { typeof(int),typeof(int) });
            il.Call(getCurrent, enumerator.Type);
            il.Stloc(temp);

            il.Ldloca(temp);
            il.Call(keyGetter, typeof(int));
            il.Stloc(currentItemKey);

            if (typeof(IBasicType).IsAssignableFrom(prop.BinaryAttr.Type.GetGenericArguments()[0]))
            {
                IBasicType t = (IBasicType)Activator.CreateInstance(prop.BinaryAttr.Type.GetGenericArguments()[0]);
                t.GetWriteILCode(prop, currentStruct, il, binaryStruct, currentItemKey, typeSize, buffer, offset, true);
            }
            else
            {
                BinaryStruct.CompileWriter(currentStruct.CurrentStorage.GetTypeInfo(typeKey, currentStruct.Scheme), il, binaryStruct, currentItemKey, buffer, offset, typeSize);
            }

            il.Ldloca(temp);
            il.Call(valueGetter);
            il.Stloc(currentItemValue);

            if (typeof(IBasicType).IsAssignableFrom(prop.BinaryAttr.Type.GetGenericArguments()[1]))
            {
                IBasicType t = (IBasicType)Activator.CreateInstance(prop.BinaryAttr.Type.GetGenericArguments()[1]);
                t.GetWriteILCode(prop, currentStruct, il, binaryStruct, currentItemValue, typeSize, buffer, offset,true);
            }
            else
            {
                BinaryStruct.CompileWriter(currentStruct.CurrentStorage.GetTypeInfo(typeValue, currentStruct.Scheme), il, binaryStruct, currentItemValue, buffer, offset, typeSize);
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
