using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public class BinaryNullable<T, TType> : IBasicType
        where T : IBasicType, new()
        where TType : struct
    {
        public Type CompareType => typeof(IList);

        public string SizeProperty { get; set; }

        private MethodInfo Getter;

        private ConstructorInfo Setter;

        public BinaryNullable()
        {
            Getter = typeof(Nullable<TType>).GetProperty("Value").GetGetMethod();

            Setter = typeof(Nullable<TType>).GetConstructor(new Type[] { typeof(TType) });
        }

        public void GetReadILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local result, GroboIL.Local typeSize, GroboIL.Local offset, bool listValue)
        {
            var exitLabel = il.DefineLabel("exit");
            BinaryStruct.ReadObjectNull(il, exitLabel, buffer, offset, typeSize);

            var val = il.DeclareLocal(typeof(TType));


            (new T()).GetReadILCode(prop, currentStruct, il, binaryStruct, buffer, val, typeSize, offset, true);

            il.Ldloc(result);
            il.Ldloc(val);
            il.Newobj(Setter);
            il.Call(prop.Setter);

            il.MarkLabel(exitLabel);
        }

        public void GetWriteILCode(PropertyData prop, BinaryStruct currentStruct, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local typeSize, GroboIL.Local buffer, GroboIL.Local offset, bool listValue)
        {
            var ilValue = il.DeclareLocal(prop.PropertyInfo.PropertyType);

            il.Ldloc(value);
            il.Call(prop.Getter);
            il.Stloc(ilValue);

            var exitLabel = il.DefineLabel("exit");

            il.Ldloca(ilValue);
            BinaryStruct.WriteNullableType<TType>(il, exitLabel, buffer, offset);

            var nval = il.DeclareLocal(typeof(TType));

            il.Ldloca(ilValue);
            il.Call(Getter);
            il.Stloc(nval);

            (new T()).GetWriteILCode(prop, currentStruct, il, binaryStruct, nval, typeSize, buffer, offset, true);

            il.MarkLabel(exitLabel);
        }
    }
}
