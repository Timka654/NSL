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
    public class BinaryTimeSpan : IBasicType
    {
        public Type CompareType => typeof(DateTime);

        private MethodInfo writeBitConverterMethodInfo;

        private MethodInfo readBitConverterMethodInfo;

        private MethodInfo propertyGetter;

        private MethodInfo timeSpanConstructor;

        public BinaryTimeSpan()
        {
            propertyGetter = typeof(TimeSpan).GetProperty("TotalMilliseconds").GetGetMethod();

            timeSpanConstructor = typeof(TimeSpan).GetMethod("FromMilliseconds",new Type[] { typeof(double) });
            
            //var d = new DateTime();
            //d.AddMilliseconds
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes",new Type[] { typeof(double) });
            readBitConverterMethodInfo = typeof(BitConverter).GetMethod("ToDouble", new Type[] { typeof(byte[]), typeof(int) });
        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var r = il.DeclareLocal(typeof(TimeSpan));
            var v = il.DeclareLocal(typeof(double));

            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Call(readBitConverterMethodInfo);
            il.Stloc(v);


            il.Ldloc(v);
            il.Call(timeSpanConstructor);

            if (listValue)
                il.Stloc(result);
            else
                il.Stloc(r);

            BinaryStruct.WriteOffsetAppend(il, offset, 8);
            if (!listValue)
            {
                il.Ldloc(result);
                il.Ldloc(r);
                il.Call(prop.Setter, isVirtual: true);
            }
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            BinaryStruct.WriteSizeChecker(il, buffer, offset, 8);
            var arr = il.DeclareLocal(typeof(byte[]));

            if (!listValue)
            {
               var temp = il.DeclareLocal(typeof(TimeSpan));

                il.Ldloc(value);
                il.Call(prop.Getter);
                il.Stloc(temp);

                il.Ldloca(temp);
            }
            else
            {
                il.Ldloca(value);
            }


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
        }
    }
}
