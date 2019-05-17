using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryArray32<T> : IBasicType
    {
        public string SizeProperty { get; set; }

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private MethodInfo codingMethodInfo;

        public BinaryArray32()
        {
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes", new Type[] { typeof(int) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToInt32", new Type[] { typeof(byte[]), typeof(int) });
            codingMethodInfo = typeof(BinaryStruct).GetProperty("Coding").GetMethod;
        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var len = il.DeclareLocal(typeof(int));
            var list = il.DeclareLocal(prop.PropertyInfo.PropertyType);

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            il.Stloc(len);

            BinaryStruct.WriteOffsetAppend(il, offset, 4);

            var type = prop.PropertyInfo.PropertyType.GetElementType();
            il.Ldloc(len);
            il.Newarr(type);

            il.Stloc(list);
            il.Ldloc(result);
            il.Ldloc(list);
            il.Call(prop.Setter, isVirtual: true);


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
                il.Ldloc(ivar);
                il.Ldloc(tempVar);
                il.Stelem(type);
            }
            else
            {
                var constr = type.GetConstructor(new Type[] { });
                if (constr == null)
                    throw new Exception($"Type {type} not have constructor with not parameters");

                il.Newobj(constr);
                il.Stloc(tempVar);

                BinaryStruct.CompileReader(currentStruct.CurrentStorage.GetTypeInfo(type, currentStruct.Scheme), il, binaryStruct, buffer, offset, tempVar, typeSize);

                il.Ldloc(list);
                il.Ldloc(ivar);
                il.Ldloc(tempVar);
                il.Stelem(type);
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
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 4);

            var arr = il.DeclareLocal(prop.PropertyInfo.PropertyType);
            var arrSize = il.DeclareLocal(typeof(byte[]));
            var len = il.DeclareLocal(typeof(int));
            il.Ldloc(value);
            il.Call(prop.Getter);
            il.Call(typeof(ICollection).GetProperty("Count").GetMethod);
            il.Stloc(len);

            il.Ldloc(value);
            il.Call(prop.Getter);
            il.Stloc(arr);

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

            var type = prop.PropertyInfo.PropertyType.GetElementType();

            var ivar = il.DeclareLocal(typeof(int));
            var currentValue = il.DeclareLocal(type);
            var point = il.DefineLabel("for_label");

            il.Ldc_I4(0);
            il.Stloc(ivar);

            il.MarkLabel(point);

            //body


            il.Ldloc(arr);
            il.Ldloc(ivar);
            il.Call(prop.PropertyInfo.PropertyType.GetMethod("Get"), isVirtual: true);
            il.Stloc(currentValue);

            if (typeof(IBasicType).IsAssignableFrom(prop.BinaryAttr.Type.GetGenericArguments()[0]))
            {
                IBasicType t = (IBasicType)Activator.CreateInstance(prop.BinaryAttr.Type.GetGenericArguments()[0]);
                t.GetWriteILCode(prop, currentStruct, il, binaryStruct, currentValue, typeSize, buffer, offset,true);
            }
            else
            {
                BinaryStruct.CompileWriter(currentStruct.CurrentStorage.GetTypeInfo(type,currentStruct.Scheme), il, binaryStruct, currentValue, buffer, offset, typeSize);
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


        }
    }
}
