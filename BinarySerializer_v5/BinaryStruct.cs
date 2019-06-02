using GrEmit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        public Func<byte[], BinaryStruct, int, Tuple<int,object>> ReadMethod { get; set; }

        public int InitLen { get; set; } = 64;

        internal TypeStorage CurrentStorage;

        public string IlReadCode;
        public string IlWriteCode;

        public BinaryStruct(Type type, string scheme, List<PropertyData> propertyList, Encoding coding, TypeStorage currentStorage)
        {
            Type = type;
            Scheme = scheme;
            Coding = coding;
            CurrentStorage = currentStorage;

            if (string.IsNullOrEmpty(scheme))
                PropertyList = propertyList;
            else
            {
                PropertyList = propertyList.Where(x => x.BinarySchemeAttrList.FirstOrDefault(y => y.SchemeName == scheme) != null).Select(y => new PropertyData(y, scheme, CurrentStorage)).ToList();

                foreach (var item in PropertyList)
                {
                    if (!item.IsBaseType)
                        item.BinaryStruct = item.BinaryStruct.GetSchemeData(scheme, Coding, CurrentStorage);
                }
            }
        }

        internal void Compile()
        {
            CompileWriter();
            CompileReader();
        }

        public BinaryStruct GetSchemeData(string schemeName, Encoding coding, TypeStorage currentStorage)
        {
            var s = new BinaryStruct(Type, schemeName, PropertyList, coding, currentStorage);
            s.Compile();
            return s;
        }

        #region Writer

        private void CompileWriter()
        {
            DynamicMethod dm = new DynamicMethod(Guid.NewGuid().ToString(), typeof(Tuple<int, byte[]>), new[] { Type, typeof(BinaryStruct) });

            using (var il = new GroboIL(dm))
            {
                var buffer = il.DeclareLocal(typeof(byte[]));

                var offset = il.DeclareLocal(typeof(int));

                var typeSize = il.DeclareLocal(typeof(int));

                var value = il.DeclareLocal(Type);

                var binaryStruct = il.DeclareLocal(typeof(BinaryStruct));

                il.Ldarg(1);
                il.Stloc(binaryStruct);

                il.Ldarg(0);
                //il.Castclass(Type);
                il.Stloc(value);

                il.Ldc_I4(InitLen);
                il.Newarr(typeof(byte));
                il.Stloc(buffer);

                CompileWriter(this, il, binaryStruct, value, buffer, offset, typeSize);

                il.Ldloc(offset);
                il.Ldloc(buffer);
                il.Newobj(typeof(Tuple<int, byte[]>).GetConstructor(new Type[] { typeof(int), typeof(byte[]) }));
                il.Ret();
                IlWriteCode = il.GetILCode();
            }

            WriteMethod = CreateWriter(dm);
        }

        public static void CompileWriter(BinaryStruct bs, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            if (bs.PropertyList.Count > 0)
            {
                foreach (var item in bs.PropertyList)
                {
                    ProcessWrite(item, bs, il, binaryStruct, value, buffer, offset, typeSize);
                }
                return;
            }
        }

        public static void ProcessWrite(PropertyData item, BinaryStruct bs, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            if (!string.IsNullOrEmpty(item.BinaryAttr?.ArraySizeName))
            {
                item.ArraySizeProperty = bs.PropertyList.Find(x => x.PropertyInfo.Name == item.BinaryAttr.ArraySizeName);
                if (item.ArraySizeProperty == null)
                    throw new Exception($"ArraySizeProperty \"{item.BinaryAttr.ArraySizeName}\" for {item.PropertyInfo.Name} in Struct {bs.Type}:{item.PropertyInfo.DeclaringType} not found(Scheme: {bs.Scheme})");
            }
            if (!string.IsNullOrEmpty(item.BinaryAttr?.TypeSizeName))
            {
                item.TypeSizeProperty = bs.PropertyList.Find(x => x.PropertyInfo.Name == item.BinaryAttr.TypeSizeName);
                if (item.TypeSizeProperty == null)
                    throw new Exception($"TypeSizeProperty \"{item.BinaryAttr.TypeSizeName}\" for {item.PropertyInfo.Name} in Struct {bs.Type}:{item.PropertyInfo.DeclaringType} not found(Scheme: {bs.Scheme})");
            }

            if (item.IsBaseType)
            {
                item.BinaryType.GetWriteILCode(item, bs, il, binaryStruct, value, typeSize, buffer, offset, false);
                return;
            }

            var methodBreak = il.DefineLabel("breakWriteMethod");

            var in_value = il.DeclareLocal(item.PropertyInfo.PropertyType);

            //значение вложенного класса
            il.Ldloc(value);
            il.Call(item.PropertyInfo.GetGetMethod());
            il.Stloc(in_value);

            WriteObjectNull(il, methodBreak, in_value, buffer, offset, typeSize);

            CompileWriter(item.BinaryStruct, il, binaryStruct, in_value, buffer, offset, typeSize);

            il.MarkLabel(methodBreak);
            //il.Pop();
        }

        #endregion

        #region Reader

        private void CompileReader()
        {
            DynamicMethod dm = new DynamicMethod(Guid.NewGuid().ToString(), typeof(Tuple<int, object>), new[] { typeof(byte[]), typeof(BinaryStruct), typeof(int) });

            using (var il = new GroboIL(dm))
            {
                var offset = il.DeclareLocal(typeof(int));

                var result = il.DeclareLocal(this.Type);

                var typeSize = il.DeclareLocal(typeof(int));
                var buffer = il.DeclareLocal(typeof(byte[]));


                var binaryStruct = il.DeclareLocal(typeof(BinaryStruct));

                il.Ldarg(1);
                il.Stloc(binaryStruct);

                var constr = GetConstructor(result.Type, null);
                if (constr == null)
                    throw new Exception($"Type {result.Type} not have constructor with not parameters");

                il.Ldarg(0);
                il.Stloc(buffer);

                il.Ldarg(2);
                il.Stloc(offset);

                il.Newobj(constr);
                il.Stloc(result);

                CompileReader(this, il, binaryStruct, buffer, offset, result, typeSize);

                il.Ldloc(offset);
                il.Ldloc(result);
                il.Newobj(typeof(Tuple<int, object>).GetConstructor(new Type[] { typeof(int), typeof(object) }));
                il.Ret();
                IlReadCode = il.GetILCode();
            }

            //ReadMethod = (ReadMethodDelegate)dm.CreateDelegate(typeof(ReadMethodDelegate));
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
                        item.BinaryType.GetReadILCode(item, bs, il, binaryStruct, buffer, result, typeSize, offset,false);
                        continue;
                    }
                    var methodBreak = il.DefineLabel("breakReadMethod");

                    ReadObjectNull(il, methodBreak, buffer, offset, typeSize);

                    var in_value = il.DeclareLocal(item.PropertyInfo.PropertyType);

                    var constr = BinaryStruct.GetConstructor(item.PropertyInfo.PropertyType,null);

                    if (constr == null)
                        throw new Exception($"Type {item.PropertyInfo.PropertyType} not have constructor with not parameters");

                    il.Newobj(constr);
                    il.Stloc(in_value);


                    CompileReader(item.BinaryStruct, il, binaryStruct, buffer, offset, in_value, typeSize);

                    il.Ldloc(result);
                    il.Ldloc(in_value);
                    il.Call(item.Setter, isVirtual: true);

                    il.MarkLabel(methodBreak);
                    //il.Pop();
                }
            }
        }

        #endregion

        #region HelpWriterMethods

        #region NULL
        public static void WriteNullableType<T>(GroboIL il, GroboIL.Label finishMethod, GroboIL.Local value, GroboIL.Local buffer, GroboIL.Local offset) 
            where T : struct
        {
            il.Ldloca(value);
            WriteNullableType<T>(il, finishMethod, buffer, offset);
        }

        public static void WriteNullableType<T>(GroboIL il, GroboIL.Label finishMethod, GroboIL.Local buffer, GroboIL.Local offset)
            where T: struct
        {
            var _null = il.DeclareLocal(typeof(bool));

            il.Call(typeof(Nullable<T>).GetProperty("HasValue").GetGetMethod());
            il.Ldc_I4(0);
            //il.Ldnull();
            il.Ceq();
            il.Stloc(_null);

            il.Ldloc(buffer);
            il.Ldloc(offset);

            il.Ldloc(_null);

            il.Stelem(typeof(byte));

            WriteOffsetAppend(il, offset, 1);

            il.Ldloc(_null);
            il.Brtrue(finishMethod);
        }

        public static void ReadNullableType(GroboIL il, GroboIL.Label finishMethod, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldelem(typeof(byte));
            WriteOffsetAppend(il, offset, 1);
            il.Brtrue(finishMethod);

        }

        public static void WriteObjectNull(GroboIL il, GroboIL.Label finishMethod, GroboIL.Local value, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            il.Ldloc(value);
            WriteObjectNull(il, finishMethod, buffer, offset, typeSize);
        }

        public static void WriteObjectNull(GroboIL il, GroboIL.Label finishMethod, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            var _null = il.DeclareLocal(typeof(bool));

            il.Ldnull();
            il.Ceq();
            il.Stloc(_null);

            il.Ldloc(buffer);
            il.Ldloc(offset);

            il.Ldloc(_null);

            il.Stelem(typeof(byte));

            WriteOffsetAppend(il, offset, 1);

            il.Ldloc(_null);
            il.Brtrue(finishMethod);
        }

        public static void ReadObjectNull(GroboIL il, GroboIL.Label finishMethod, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldelem(typeof(byte));
            WriteOffsetAppend(il, offset, 1);
            il.Brtrue(finishMethod);

        }

        #endregion

        #region Size

        public static void WriteSizeChecker(GroboIL il, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            //il.Ldloca(buffer);
            //il.Ldloc(offset);
            //il.Ldloc(typeSize);
            //il.Call(resizeMethod);
            il.Ldloc(typeSize);
            Resize(il, buffer, offset);
        }

        public static void WriteSizeChecker(GroboIL il, GroboIL.Local buffer, GroboIL.Local offset, int len)
        {
            //il.Ldloca(buffer);
            //il.Ldloc(offset);
            //il.Ldc_I4(len);
            //il.Call(resizeMethod);
            il.Ldc_I4(len);
            Resize(il, buffer, offset);
        }

        #endregion

        #region Offset

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

        #endregion

        internal static void Resize(GroboIL il, GroboIL.Local buffer, GroboIL.Local offset/*, GroboIL.Local len*/)
        {
            var totalLen = il.DeclareLocal(typeof(int));
            //offset add typesize = minSize
            il.Ldloc(offset);
            il.Add();
            il.Stloc(totalLen);


            var exit = il.DefineLabel("exit");

            //Compare if minSize < buffer.len
            il.Ldloc(totalLen);
            il.Ldloc(buffer);
            il.Ldlen();
            il.Clt (false);
            il.Brtrue(exit);


            var avar = il.DeclareLocal(typeof(byte));

            il.Ldc_I4(2);
            il.Stloc(avar);

            var point = il.DefineLabel("for_label");

            il.MarkLabel(point);

            //body

            il.Ldloc(avar);
            il.Ldc_I4(2);
            il.Mul();
            il.Stloc(avar);

            //end body


            il.Ldloc(buffer);
            il.Ldlen();
            il.Ldloc(avar);
            il.Mul();
            //OutputValue(il,typeof(int));

            il.Ldloc(totalLen);
            //OutputValue(il, typeof(int));

            il.Clt(false);
            //OutputValue(il, typeof(bool));
            il.Brtrue(point);

            il.Ldloca(buffer);

            il.Ldloc(avar);
            il.Ldloc(buffer);
            il.Ldlen();
            il.Mul();
            //OutputValue(il, typeof(int));

            il.Call(typeof(Array).GetMethod("Resize", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(typeof(byte)));
            
            
            il.MarkLabel(exit);
        }

        //public static void Resize(ref byte[] buffer, int offset, int applen)
        //{
        //    int nlen = buffer.Length;
        //    while (nlen <= offset + applen)
        //    {
        //        nlen *= 2;
        //    }
        //    Array.Resize(ref buffer, nlen);
        //}

        #endregion

        #region LambdaCreators

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

        private static Func<byte[], BinaryStruct, int, Tuple<int, object>> CreateReader(MethodInfo method)
        {
            var param = Expression.Parameter(typeof(byte[]), "e");
            var param1 = Expression.Parameter(typeof(BinaryStruct), "e1");
            var param2 = Expression.Parameter(typeof(int), "e2");

            Expression body = Expression.Convert(Expression.Call(method, param, param1, param2), typeof(Tuple<int, object>));
            var getterExpression = Expression.Lambda<Func<byte[], BinaryStruct, int, Tuple<int, object>>>(body, param, param1, param2);
            return getterExpression.Compile();
        }

        #endregion        

        public static void OutputValue(GroboIL il,Type t)
        {
            il.Dup();
            //il.Box(t);
            il.Call(typeof(BinaryStruct).GetMethod("OutputBuffer", BindingFlags.NonPublic | BindingFlags.Static));
        }

        private static void OutputBuffer(byte[] buf)
        {

        }

        internal static ConstructorInfo GetConstructor(Type type, Type[] args)
        {
           var construct = type.GetConstructors(BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            if (args == null)
                return construct.OrderBy(x => x.GetParameters().Count() != 0).First();

            return construct.First(x => x.GetParameters().Select(y=>y.ParameterType).SequenceEqual(args));
        }
    }
}
