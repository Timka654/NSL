using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryList<T> : IBasicType
    {
        public Type CompareType => null;

        public string SizeProperty { get; set; }

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private MethodInfo codingMethodInfo;

        public BinaryList()
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

            il.Newobj(BinaryStruct.GetConstructor(prop.PropertyInfo.PropertyType, null));

            il.Stloc(list);
            il.Ldloc(result);
            il.Ldloc(list);
            il.Call(prop.Setter, isVirtual: true);

            il.Ldloc(len);
            il.Ldc_I4(0);
            il.Ceq();
            il.Brtrue(exitLabel);

            var type = prop.PropertyInfo.PropertyType.GetGenericArguments()[0];

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
                il.Call(prop.PropertyInfo.PropertyType.GetMethod("Add"), isVirtual: true);
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
                il.Call(prop.PropertyInfo.PropertyType.GetMethod("Add"), isVirtual:true);
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
            il.Pop();
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            var arr = il.DeclareLocal(prop.PropertyInfo.PropertyType);
            var len = il.DeclareLocal(typeof(int));


            if (prop.PropertyInfo != null)
            {
                il.Ldloc(value);
                il.Call(prop.ArraySizeProperty.Getter);
                il.Stloc(len);
            }
            else
            {
                il.Ldc_I4(prop.ArraySize);
                il.Stloc(len);
            }

            il.Ldloc(value);
            il.Call(prop.Getter);
            il.Stloc(arr);

            var type = prop.PropertyInfo.PropertyType.GetGenericArguments()[0];

            var ivar = il.DeclareLocal(typeof(int));
            var currentValue = il.DeclareLocal(type);
            var point = il.DefineLabel("for_label");

            il.Ldc_I4(0);
            il.Stloc(ivar);

            il.MarkLabel(point);

            //body


            il.Ldloc(arr);
            il.Ldloc(ivar);
            il.Call(prop.PropertyInfo.PropertyType.GetMethod("get_Item"), isVirtual: true);
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
