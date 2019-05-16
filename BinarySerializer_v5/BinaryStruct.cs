﻿using GrEmit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BinarySerializer
{
    public class BinaryStruct
    {
        public Type Type { get; set; }

        public string Scheme { get; set; }
        public Encoding Coding { get; set; }

        public List<PropertyData> PropertyList { get; set; }

        public Func<object, BinaryStruct, Tuple<int, byte[]>> WriteMethod { get; set; }

        public Func<byte[], BinaryStruct, object> ReadMethod { get; set; }

        public int InitLen { get; set; } = 32;

        private static MethodInfo resizeMethod;

        private TypeStorage CurrentStorage;

        public BinaryStruct(Type type, string scheme, List<PropertyData> propertyList, Encoding coding, TypeStorage currentStorage)
        {
            Type = type;
            Scheme = scheme;
            Coding = coding;
            CurrentStorage = currentStorage;

            if (string.IsNullOrEmpty(scheme))
                PropertyList = propertyList;
            else
                PropertyList = propertyList.Where(x => x.BinarySchemeAttrList.FirstOrDefault(y => y.SchemeName == scheme) != null).ToList();

            if (resizeMethod == null)
                resizeMethod = this.GetType().GetMethod("Resize", BindingFlags.Public | BindingFlags.Static);

        }

        internal void Compile()
        {
            CompileWriter();
            CompileReader();
        }

        public BinaryStruct GetSchemeData(string schemeName, Encoding coding, TypeStorage currentStorage)
        {
            return new BinaryStruct(Type, schemeName, PropertyList, coding, currentStorage);
        }

        private void CompileWriter()
        {
            DynamicMethod dm = new DynamicMethod(Guid.NewGuid().ToString(), typeof(Tuple<int, byte[]>), new[] { Type, typeof(BinaryStruct) });

            using (var il = new GroboIL(dm))
            {
                var arr = il.DeclareLocal(typeof(byte[]));

                var offset = il.DeclareLocal(typeof(int));

                var typeSize = il.DeclareLocal(typeof(int));

                var value = il.DeclareLocal(Type);

                var binaryStruct = il.DeclareLocal(typeof(BinaryStruct));

                il.Ldarg(1);
                il.Stloc(binaryStruct);

                il.Ldarg(0);
                il.Stloc(value);

                il.Ldc_I4(InitLen);
                il.Newarr(typeof(byte));
                il.Stloc(arr);

                CompileWriter(this, il, binaryStruct, value, arr, offset, typeSize);
            }

            WriteMethod = CreateWriter(dm);
        }

        public static void CompileWriter(BinaryStruct bs, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            if (bs.PropertyList.Count > 0)
            {
                foreach (var item in bs.PropertyList)
                {
                    if (item.IsBaseType)
                    {
                        item.BinaryType.GetWriteILCode(item, bs, il,binaryStruct, value, typeSize, buffer, offset);
                        continue;
                    }

                    var in_value = il.DeclareLocal(item.PropertyInfo.PropertyType);
                    //var in_binary = il.DeclareLocal(typeof(BinaryStruct));

                    //значение вложенного класса
                    il.Ldloc(value);
                    il.Call(item.PropertyInfo.GetGetMethod());
                    il.Stloc(in_value);

                    // Бинарная структура которая относится к этому вложеному классу
                    //il.Ldloc(value);
                    //il.Call(item.GetType().GetProperty("BinaryStruct").GetGetMethod());
                    //il.Stloc(in_binary);

                    CompileWriter(item.BinaryStruct, il, binaryStruct, in_value, buffer, offset, typeSize);


                    //CompileWriter(TypeStorage.Instance.GetTypeInfo(item.PropertyInfo.PropertyType, bs.Scheme), il, buffer, offset, typeSize);
                }

                il.Ldloc(offset);
                il.Ldloc(buffer);
                il.Newobj(typeof(Tuple<int, byte[]>).GetConstructor(new Type[] { typeof(int), typeof(byte[]) }));
                il.Ret();
            }
            else
            {
                il.Ldc_I4(0);
                il.Ldloc(buffer);
                il.Newobj(typeof(Tuple<int, byte[]>).GetConstructor(new Type[] { typeof(int), typeof(byte[]) }));
                il.Ret();
            }
        }

        private void CompileReader()
        {
            DynamicMethod dm = new DynamicMethod(Guid.NewGuid().ToString(), this.Type, new[] { typeof(byte[]), typeof(BinaryStruct) });

            using (var il = new GroboIL(dm))
            {
                var offset = il.DeclareLocal(typeof(int));

                var result = il.DeclareLocal(this.Type);

                var typeSize = il.DeclareLocal(typeof(int));
                var buffer = il.DeclareLocal(typeof(byte[]));


                var binaryStruct = il.DeclareLocal(typeof(BinaryStruct));

                il.Ldarg(1);
                il.Stloc(binaryStruct);

                var constr = result.Type.GetConstructor(new Type[] { });
                if (constr == null)
                    throw new Exception($"Type {result.Type} not have constructor with not parameters");

                il.Ldarg(0);
                il.Stloc(buffer);

                il.Newobj(constr);
                il.Stloc(result);

                CompileReader(this, il, binaryStruct, buffer, offset, result, typeSize);

                il.Ldloc(result);
                il.Ret();
            }

            ReadMethod = CreateReader(dm);
        }

        public static void CompileReader(BinaryStruct bs, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local result, GroboIL.Local typeSize)
        {
            if (bs.PropertyList.Count > 0)
            {
                foreach (var item in bs.PropertyList)
                {
                    if (item.IsBaseType)
                    {
                        item.BinaryType.GetReadILCode(item, bs, il, binaryStruct, buffer, result, typeSize, offset);
                        continue;
                    }

                    var in_value = il.DeclareLocal(item.PropertyInfo.PropertyType);

                    var constr = item.PropertyInfo.PropertyType.GetConstructor(new Type[] { });

                    if (constr == null)
                        throw new Exception($"Type {item.PropertyInfo.PropertyType} not have constructor with not parameters");

                    il.Newobj(constr);
                    il.Stloc(in_value);


                    CompileReader(item.BinaryStruct, il, binaryStruct, buffer, offset, in_value, typeSize);

                    il.Ldloc(result);
                    il.Ldloc(in_value);
                    il.Call(item.Setter, isVirtual: true);

                }
            }
        }

        private static Func<object, BinaryStruct, Tuple<int, byte[]>> CreateWriter(MethodInfo method)
        {
            var param = Expression.Parameter(typeof(object), "e");
            var param1 = Expression.Parameter(method.GetParameters()[1].ParameterType);

            Expression body = Expression.Convert(
                Expression.Call(method,
                    Expression.Convert(param,
                        method.GetParameters()[0].ParameterType),
                    param1
                    ),
                typeof(Tuple<int, byte[]>));

            var getterExpression = Expression.Lambda<Func<object, BinaryStruct, Tuple<int, byte[]>>>(body, param, param1);
            return getterExpression.Compile();
        }

        private static Func<byte[], BinaryStruct, object> CreateReader(MethodInfo method)
        {
            var param = Expression.Parameter(typeof(byte[]), "e");
            var param1 = Expression.Parameter(typeof(BinaryStruct), "e1");

            Expression body = Expression.Convert(Expression.Call(method, param, param1), typeof(object));
            var getterExpression = Expression.Lambda<Func<byte[], BinaryStruct, object>>(body, param, param1);
            return getterExpression.Compile();
        }


        public static void WriteSizeChecker(GroboIL il, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            il.Ldloca(buffer);
            il.Ldloc(offset);
            il.Ldloc(typeSize);
            il.Call(resizeMethod);
        }

        public static void WriteSizeChecker(GroboIL il, GroboIL.Local buffer, GroboIL.Local offset, int len)
        {
            il.Ldloca(buffer);
            il.Ldloc(offset);
            il.Ldc_I4(len);
            il.Call(resizeMethod);
        }

        public static void WriteOffsetAppend(GroboIL il, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            il.Ldloc(offset);
            il.Ldloc(typeSize);
            il.Add();
            il.Stloc(offset);
        }
        public static void WriteOffsetAppend(GroboIL il, GroboIL.Local offset, int len)
        {
            il.Ldloc(offset);
            il.Ldc_I4(len);
            il.Add();
            il.Stloc(offset);
        }

        public static void Resize(ref byte[] buffer, int offset, int applen)
        {
            while (buffer.Length < offset + applen)
            {
                Array.Resize(ref buffer, buffer.Length * 2);
            }
        }
    }
}