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
    public class BinaryNullableDateTime : IBasicType
    {
        public Type CompareType => typeof(DateTime);

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private MethodInfo propertyGetter;

        private MethodInfo substractMethod;

        private MethodInfo addMethod;

        private ConstructorInfo datetimeConstructor;

        public BinaryNullableDateTime()
        {
            addMethod = typeof(DateTime).GetMethod("AddMilliseconds", new Type[] { typeof(double) });
            substractMethod = typeof(DateTime).GetMethod("Subtract", new Type[] { typeof(DateTime) });
            propertyGetter = typeof(TimeSpan).GetProperty("TotalMilliseconds").GetGetMethod();
            datetimeConstructor = typeof(DateTime).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });
            //var d = new DateTime();
            //d.AddMilliseconds
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes",new Type[] { typeof(double) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToDouble", new Type[] { typeof(byte[]), typeof(int) });
        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var r = il.DeclareLocal(typeof(DateTime));

            var finish = il.DefineLabel("null_finish");

            BinaryStruct.ReadNullableType(il, finish, buffer, offset, typeSize);

            var v = il.DeclareLocal(typeof(double));

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            il.Stloc(v);

            il.Ldloca(r);
            il.Ldc_I4(1970);
            il.Ldc_I4(1);
            il.Ldc_I4(1);
            il.Ldc_I4(0);
            il.Ldc_I4(0);
            il.Ldc_I4(0);
            il.Ldc_I4(0);
            il.Call(datetimeConstructor);

            il.Ldloca(r);
            il.Ldloc(v);
            il.Call(addMethod);

            if (listValue)
                il.Stloc(result);
            else
                il.Stloc(r);

            BinaryStruct.WriteOffsetAppend(il, offset, 8);
            if (!listValue)
            {

                var temp = il.DeclareLocal(typeof(DateTime?));

                il.Ldloca(temp);
                il.Ldloc(r);
                il.Call(typeof(Nullable<DateTime>).GetConstructor(new Type[] { typeof(DateTime) }));


                il.Ldloc(result);
                il.Ldloc(temp);
                il.Call(prop.Setter, isVirtual: true);
            }
            il.MarkLabel(finish);
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 9);
            var arr = il.DeclareLocal(typeof(byte[]));
            var finish = il.DefineLabel("null_finish");
            var v = il.DeclareLocal(typeof(DateTime));
            var t = il.DeclareLocal(typeof(TimeSpan));
            if (!listValue)
            {
                il.Ldloc(value);
                var v1 = il.DeclareLocal(typeof(DateTime?));
                il.Call(prop.Getter);
                il.Stloc(v1);
                il.Ldloca(v1);
            }
            else
            {
                il.Ldloca(value);
            }
            BinaryStruct.WriteNullableType<DateTime>(il, finish, buffer, offset, typeSize);

            if (!listValue)
            {
                il.Ldloc(value);
                var v1 = il.DeclareLocal(typeof(DateTime?));
                il.Call(prop.Getter);
                il.Stloc(v1);
                il.Ldloca(v1);
            }
            else
            {
                il.Ldloca(value);
            }
            il.Call(typeof(DateTime?).GetProperty("Value").GetGetMethod());


            var v2 = il.DeclareLocal(typeof(DateTime));
            il.Stloc(v2);

            il.Ldloca(v);
            il.Ldc_I4(1970);
            il.Ldc_I4(1);
            il.Ldc_I4(1);
            il.Ldc_I4(0);
            il.Ldc_I4(0);
            il.Ldc_I4(0);
            il.Ldc_I4(0);
            il.Call(datetimeConstructor);

            il.Ldloca(v2);
            il.Ldloc(v);
            il.Call(substractMethod);
            il.Stloc(t);
            il.Ldloca(t);
            il.Call(propertyGetter);

            il.Call(writeBitConverterMethodInfo);
            il.Stloc(arr);

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldloc(arr);
            il.Ldc_I4(0);
            il.Ldelem(typeof(byte));
            il.Stelem(typeof(byte));

            for (int i = 1; i < 8; i++)
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
            BinaryStruct.WriteOffsetAppend(il, offset, 8);
            il.MarkLabel(finish);
        }
    }
}
