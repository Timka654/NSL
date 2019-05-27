using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;
using GrEmit;
using GrEmit.Utils;
#if NOT_UNITY
using System.Numerics;
#else
using UnityEngine;
#endif

namespace BinarySerializer.DefaultTypes
{
    public class BinaryNullableVector3 : IBasicType
    {
        public Type CompareType => typeof(Vector3);

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private ConstructorInfo initialConstructor;

        private FieldInfo xField;

        private FieldInfo yField;

        private FieldInfo zField;

        public BinaryNullableVector3()
        {
#if NOT_UNITY
            xField = typeof(Vector3).GetField("X");
            yField = typeof(Vector3).GetField("Y");
            zField = typeof(Vector3).GetField("Z");
#else
            
            xField = typeof(Vector3).GetField("x");
            yField = typeof(Vector3).GetField("y");
            zField = typeof(Vector3).GetField("z");
#endif

            initialConstructor = typeof(Vector3).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float) });
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes", new Type[] { typeof(float) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToSingle", new Type[] { typeof(byte[]), typeof(int) });

        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var r = il.DeclareLocal(typeof(Vector3));

            var finish = il.DefineLabel("null_finish");

            BinaryStruct.ReadNullableType(il, finish, buffer, offset, typeSize);

            if (listValue)
                il.Ldloca(result);
            else
                il.Ldloca(r);

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldc_I4(4);
            il.Add();
            il.Call(readBitConverterMethodInfo);

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldc_I4(8);
            il.Add();
            il.Call(readBitConverterMethodInfo);

            il.Call(initialConstructor);

            //if (listValue)
            //    il.Stloc(result);
            //else
            //    il.Stloc(r);

            BinaryStruct.WriteOffsetAppend(il, offset, 12);

            if (!listValue)
            {
                il.Ldloc(result);

                var temp = il.DeclareLocal(typeof(Vector3?));

                il.Ldloca(temp);
                il.Ldloc(r);

                il.Call(typeof(Nullable<Vector3>).GetConstructor(new Type[] { typeof(Vector3) }));

                il.Ldloc(temp);
                il.Call(prop.Setter, isVirtual: true);
            }

            il.MarkLabel(finish);
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 13);
            var v = il.DeclareLocal(typeof(Vector3?));
            var arr = il.DeclareLocal(typeof(byte[]));
            var finish = il.DefineLabel("null_finish");

            il.Ldloc(value);
            if (!listValue)
                il.Call(prop.Getter);
            il.Stloc(v);
            il.Ldloca(v);
            //il.Dup();
            BinaryStruct.WriteNullableType<Vector3>(il, finish, buffer, offset, typeSize);

            var temp2 = il.DeclareLocal(typeof(Vector3));

            if (!listValue)
            {
                var temp = il.DeclareLocal(typeof(Vector3?));

                il.Ldloc(value);
                il.Call(prop.Getter);
                il.Stloc(temp);

                il.Ldloca(temp);
            }
            else
            {
                il.Ldloca(value);
            }

            il.Call(typeof(Vector3?).GetProperty("Value").GetGetMethod());
            il.Stloc(temp2);

            il.Ldloca(temp2);
            //il.Dup();
            //il.Box(typeof(Vector2));
            il.Ldfld(xField);

            //il.Dup();
            //il.Pop();
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

            il.Ldloca(temp2);

            il.Ldfld(yField);
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

            il.Ldloca(temp2);

            il.Ldfld(zField);
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
            il.MarkLabel(finish);
        }
    }
}
