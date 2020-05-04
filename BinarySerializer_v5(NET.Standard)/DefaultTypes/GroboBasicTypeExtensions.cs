using GrEmit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    internal static class GroboBasicTypeExtensions
    {
        private static readonly MethodInfo writeMethod = typeof(System.IO.MemoryStream).GetMethod("Write", new Type[] { typeof(byte[]), typeof(int), typeof(int) });
        private static readonly MethodInfo writeByteMethod = typeof(System.IO.MemoryStream).GetMethod("WriteByte", new Type[] { typeof(byte) });

        public static void PropertyGetter(this BinaryMemberData member, GroboIL il)
        {
            if (member.MemberInfo is FieldInfo fi)
            {
                il.Ldfld(fi);
                return;
            }
            il.Call(member.Getter, isVirtual: member.Getter.IsVirtual);
        }

        public static void PropertySetter(this BinaryMemberData member, GroboIL il)
        {
            if (member.MemberInfo is FieldInfo fi)
            {
                il.Stfld(fi);
                return;
            }
            il.Call(member.Setter, isVirtual: member.Setter.IsVirtual);
        }

        public static void ArraySetter(this GroboIL il, GroboIL.Local memoryStream, GroboIL.Local arr, int len)
        {
            il.Ldloc(memoryStream);
            il.Ldloc(arr);
            il.Ldc_I4(0);
            il.Ldc_I4(len);

            il.Call(writeMethod, isVirtual: writeMethod.IsVirtual);
        }

        public static void ArrayByteSetter(this GroboIL il, GroboIL.Local memoryStream, GroboIL.Local _byte)
        {
            il.Ldloc(memoryStream);
            il.Ldloc(_byte);

            il.Call(writeByteMethod, isVirtual: writeByteMethod.IsVirtual);
        }

        public static void ArraySetter(this GroboIL il, GroboIL.Local memoryStream, GroboIL.Local arr, GroboIL.Local len)
        {
            il.Ldloc(memoryStream);
            il.Ldloc(arr);
            il.Ldc_I4(0);
            il.Ldloc(len);

            il.Call(writeMethod, isVirtual: writeMethod.IsVirtual);
        }

        public static object GetDefaultValue(this Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }
    }
}
