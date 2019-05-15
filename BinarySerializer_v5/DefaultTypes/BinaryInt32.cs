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
    public class BinaryInt32 : IBasicType
    {
        public int TypeSize { get;  set; }

        public string SizeProperty { get; set; }

        private MethodInfo writeBitConverterMethodInfo;

        public BinaryInt32()
        {
            writeBitConverterMethodInfo = typeof(BitConverter).GetMethod("GetBytes",new Type[] { typeof(int) });
        }

        public void GetReadILCode(PropertyData prop, Type currentType, GroboIL il)
        {
            //var val = il.DeclareLocal(typeof(int));

            //il.Ldarg(0);
            //il.Call(prop.Getter);
            //il.Stloc(val);

            //il.Ldloc(val);
            //il.Call(writeBitConverterMethodInfo);

        }

        public void GetWriteILCode(PropertyData prop, Type currentType, GroboIL il)
        {
            var val = il.DeclareLocal(typeof(int));

            il.Ldarg(0);
            il.Call(prop.Getter);
            il.Stloc(val);

            il.Ldloc(val);
            il.Call(writeBitConverterMethodInfo);
        }
    }
}
