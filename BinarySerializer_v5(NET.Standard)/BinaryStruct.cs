using BinarySerializer.DefaultTypes;
using GrEmit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BinarySerializer
{
    internal class TempValue
    {
        public GroboIL.Local Value { get; set; }

        public int Size { get; set; }
    }

    /// <summary>
    /// Данные для бинарной серриализации класса
    /// </summary>
    public class BinaryStruct
    {
        internal Dictionary<string, TempValue> TempBuildValues = new Dictionary<string, TempValue>();

        /// <summary>
        /// Тип класса
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Схема
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Кодировка (указываеться до вызова компиляции)
        /// </summary>
        public Encoding Coding { get; set; }

        /// <summary>
        /// Список учасников
        /// </summary>
        public List<BinaryMemberData> PropertyList { get; set; }

        /// <summary>
        /// Метод серриализации
        /// </summary>
        public Func<object, BinaryStruct, Tuple<int, byte[]>> WriteMethod { get; set; }

        /// <summary>
        /// Метод десерриалзации
        /// </summary>
        public Func<byte[], BinaryStruct, int, Tuple<int, object>> ReadMethod { get; set; }

        /// <summary>
        /// Размер буффера серриализации (указываеться до вызова компиляции)
        /// </summary>
        public int InitLen { get; set; } = 64;

        internal TypeStorage CurrentStorage;

#if DEBUG
        
        /// <summary>
        /// Сгенерированный код чтения бинарных данных (отладка)
        /// </summary>
        public string IlReadCode;

        /// <summary>
        /// Сгенерированный код записи бинарных данных (отладка)
        /// </summary>
        public string IlWriteCode;
#endif

        /// <summary>
        /// Совмещение присутствующих членов при обьявлении в нескольких местах с разными данными 
        /// </summary>
        /// <param name="old"></param>
        /// <param name="propertyList"></param>
        /// <returns></returns>
        private List<BinaryMemberData> FillPropertyes(BinaryStruct old, List<BinaryMemberData> propertyList)
        {
            List<BinaryMemberData> result = old.PropertyList.ToList();

            BinaryMemberData tempPropData = null;

            foreach (var item in propertyList)
            {
                if ((tempPropData = result.FirstOrDefault(x => x.Name == item.Name)) == null)
                {
                    result.Add(item);
                }
                else
                {
                    if (tempPropData.BinaryAttr.Type != item.BinaryAttr.Type)
                        throw new Exception($"Cannot fill property \"{tempPropData.Name}\" in struct {Type} because of old binary type ({tempPropData.BinaryAttr.Type}) not compared new type ({item.BinaryAttr.Type})");

                    if (tempPropData.BinaryAttr.ArraySize != item.BinaryAttr.ArraySize)
                        throw new Exception($"Cannot fill property \"{tempPropData.Name}\" in struct {Type} because of old array size ({tempPropData.BinaryAttr.ArraySize}) not compared new array size ({item.BinaryAttr.ArraySize})");

                    if (tempPropData.BinaryAttr.ArraySizeName != item.BinaryAttr.ArraySizeName)
                        throw new Exception($"Cannot fill property \"{tempPropData.Name}\" in struct {Type} because of old array size name ({tempPropData.BinaryAttr.ArraySizeName}) not compared new array size name ({item.BinaryAttr.ArraySizeName})");


                    if (tempPropData.BinaryAttr.TypeSize != item.BinaryAttr.TypeSize)
                        throw new Exception($"Cannot fill property \"{tempPropData.Name}\" in struct {Type} because of old type size ({tempPropData.BinaryAttr.TypeSize}) not compared new type size ({item.BinaryAttr.TypeSize})");

                    if (tempPropData.BinaryAttr.TypeSizeName != item.BinaryAttr.TypeSizeName)
                        throw new Exception($"Cannot fill property \"{tempPropData.Name}\" in struct {Type} because of old type size name ({tempPropData.BinaryAttr.TypeSizeName}) not compared new type size name ({item.BinaryAttr.TypeSizeName})");

                    foreach (var scheme in item.BinarySchemeAttrList)
                    {
                        if (tempPropData.BinarySchemeAttrList.Exists(x => x.SchemeName == scheme.SchemeName))
                            continue;
                        tempPropData.BinarySchemeAttrList.Add(scheme);
                    }

                }
            }

            return result;
        }

        public BinaryStruct(Type type, string scheme, List<BinaryMemberData> propertyList, Encoding coding, TypeStorage currentStorage, bool builderCompile = false)
        {
            Type = type;
            Scheme = scheme;
            Coding = coding;
            CurrentStorage = currentStorage;

            if (string.IsNullOrEmpty(scheme))
            {
                var existOldType = builderCompile ? currentStorage.GetTypeInfo(type, "") : null;
                if (existOldType == null)
                {
                    PropertyList = propertyList;
                }
                else
                {
                    PropertyList = FillPropertyes(existOldType, propertyList);
                }
                PropertyList = PropertyList.OrderBy(x => x.Name).ToList();
            }
            else
            {
                PropertyList = propertyList.Where(x => x.BinarySchemeAttrList.FirstOrDefault(y => y.SchemeName == scheme) != null).Select(y => new BinaryMemberData(y, scheme, CurrentStorage)).ToList();

                foreach (var item in PropertyList)
                {
                    if (!item.IsBaseType)
                        item.BinaryStruct = item.BinaryStruct.GetSchemeData(scheme, Coding, CurrentStorage);
                }
            }
        }

        /// <summary>
        /// Компиляция методов чтения и записи
        /// </summary>
        internal void Compile()
        {
            CompileWriter();
            CompileReader();
        }

        /// <summary>
        /// Компиляция структуры для указанной схемы
        /// </summary>
        /// <param name="schemeName">Схема</param>
        /// <param name="coding">Кодировка</param>
        /// <param name="currentStorage">Текущее хранилище откуда будут браться вложенные сложные типы</param>
        /// <returns></returns>
        public BinaryStruct GetSchemeData(string schemeName, Encoding coding, TypeStorage currentStorage)
        {
            var s = new BinaryStruct(Type, schemeName, PropertyList, coding, currentStorage);
            s.Compile();
            return s;
        }

        /// <summary>
        /// Получить временные значения
        /// </summary>
        /// <param name="il"></param>
        private void GetTempValues(GroboIL il)
        {
            TempBuildValues.Clear();

            var tempBuffer = new TempValue() { Value = il.DeclareLocal(typeof(byte[])) };

            TempBuildValues.Add("tempBuffer", tempBuffer);

            il.Ldc_I4(16);
            il.Newarr(typeof(byte));
            il.Stloc(tempBuffer.Value);


            tempBuffer = new TempValue() { Value = il.DeclareLocal(typeof(byte[])) };

            TempBuildValues.Add("tempLenghtBuffer", tempBuffer);
        }

        #region Writer

        /// <summary>
        /// Скомпилировать метод для записи
        /// </summary>
        private void CompileWriter()
        {
            DynamicMethod dm = new DynamicMethod(Guid.NewGuid().ToString(), typeof(Tuple<int, byte[]>), new[] { Type, typeof(BinaryStruct) });

            using (var il = new GroboIL(dm))
            {
                var buffer = il.DeclareLocal(typeof(MemoryStream));

                var typeSize = il.DeclareLocal(typeof(int));

                var value = il.DeclareLocal(Type);

                var binaryStruct = il.DeclareLocal(typeof(BinaryStruct));

                il.Ldarg(1);
                il.Stloc(binaryStruct);

                il.Ldarg(0);
                il.Stloc(value);
                il.Ldc_I4(InitLen);
                il.Newobj(typeof(MemoryStream).GetConstructor(new Type[] { typeof(int) }));
                il.Stloc(buffer);

                CompileWriter(this, il, binaryStruct, value, buffer, typeSize);

                il.Ldloc(buffer);
                il.Call(typeof(MemoryStream).GetProperty("Length").GetGetMethod());
                il.Conv<int>();

                il.Ldloc(buffer);
                il.Call(typeof(MemoryStream).GetMethod("ToArray"));

                il.Newobj(typeof(Tuple<int, byte[]>).GetConstructor(new Type[] { typeof(int), typeof(byte[]) }));
                il.Ret();

#if DEBUG
                IlWriteCode = il.GetILCode();
#endif
            }

            WriteMethod = CreateWriter(dm);
        }

        /// <summary>
        /// Построить код записи для структуры
        /// </summary>
        /// <param name="bs">Структура</param>
        /// <param name="il">Генератор</param>
        /// <param name="binaryStruct">Структура</param>
        /// <param name="value">Обьект для серриализации</param>
        /// <param name="buffer">Массив байт для возврата</param>
        /// <param name="typeSize">Размер данныз для возврата</param>
        public static void CompileWriter(BinaryStruct bs, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local buffer, GroboIL.Local typeSize)
        {
            if (bs.PropertyList.Count > 0)
            {
                bs.GetTempValues(il);

                foreach (var item in bs.PropertyList)
                {
                    if (item.Setter == null)
                        continue;
                    ProcessWrite(item, bs, il, binaryStruct, value, buffer, typeSize);
                }
                return;
            }
        }

        /// <summary>
        /// Построить код записи для учасника структуры
        /// </summary>
        /// <param name="item">Данные об учаснике</param>
        /// <param name="bs">Структура</param>
        /// <param name="il">Генератор</param>
        /// <param name="binaryStruct">Структура</param>
        /// <param name="value">Обьект для серриализации</param>
        /// <param name="buffer">Массив байт для возврата</param>
        /// <param name="typeSize">Размер данныз для возврата</param>
        public static void ProcessWrite(BinaryMemberData item, BinaryStruct bs, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local value, GroboIL.Local buffer, GroboIL.Local typeSize)
        {
            if (!string.IsNullOrEmpty(item.BinaryAttr?.ArraySizeName))
            {
                item.ArraySizeProperty = bs.PropertyList.Find(x => x.Name == item.BinaryAttr.ArraySizeName);
                if (item.ArraySizeProperty == null)
                    throw new Exception($"ArraySizeProperty \"{item.BinaryAttr.ArraySizeName}\" for {item.Name} in Struct {bs.Type}:{item.DeclaringType} not found(Scheme: {bs.Scheme})");
            }
            if (!string.IsNullOrEmpty(item.BinaryAttr?.TypeSizeName))
            {
                item.TypeSizeProperty = bs.PropertyList.Find(x => x.Name == item.BinaryAttr.TypeSizeName);
                if (item.TypeSizeProperty == null)
                    throw new Exception($"TypeSizeProperty \"{item.BinaryAttr.TypeSizeName}\" for {item.Name} in Struct {bs.Type}:{item.DeclaringType} not found(Scheme: {bs.Scheme})");
            }

            if (item.IsBaseType)
            {
                item.BinaryType.GetWriteILCode(item, bs, il, binaryStruct, value, typeSize, buffer, false);
                return;
            }

            var methodBreak = il.DefineLabel("breakWriteMethod");

            var in_value = il.DeclareLocal(item.Type);

            //значение вложенного класса
            il.Ldloc(value);
            il.Call(item.Getter);
            il.Stloc(in_value);

            WriteObjectNull(bs, il, methodBreak, in_value, buffer, typeSize);

            CompileWriter(item.BinaryStruct, il, binaryStruct, in_value, buffer, typeSize);

            il.MarkLabel(methodBreak);
        }

        #endregion

        #region Reader

        /// <summary>
        /// Скомпилировать метод для чтения
        /// </summary>
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

#if DEBUG
                IlReadCode = il.GetILCode();
#endif
            }

            ReadMethod = CreateReader(dm);
        }

        /// <summary>
        /// Построить код чтения для структуры
        /// </summary>
        /// <param name="bs">Структура</param>
        /// <param name="il">Генератор</param>
        /// <param name="binaryStruct">Структура</param>
        /// <param name="buffer">Массив байт для чтения</param>
        /// <param name="offset">Позиция в буффере</param>
        /// <param name="result">Результат дессериализации</param>
        /// <param name="typeSize">temp type size variable</param>
        public static void CompileReader(BinaryStruct bs, GroboIL il, GroboIL.Local binaryStruct, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local result, GroboIL.Local typeSize)
        {
            if (bs.PropertyList.Count > 0)
            {
                bs.GetTempValues(il);
                foreach (var item in bs.PropertyList)
                {
                    if (item.Getter == null)
                        continue;

                    if (item.IsBaseType)
                    {
                        item.BinaryType.GetReadILCode(item, bs, il, binaryStruct, buffer, result, typeSize, offset, false);
                        continue;
                    }
                    var methodBreak = il.DefineLabel("breakReadMethod");

                    ReadObjectNull(il, methodBreak, buffer, offset, typeSize);

                    var in_value = il.DeclareLocal(item.Type);

                    var constr = BinaryStruct.GetConstructor(item.Type, null);

                    if (constr == null)
                        throw new Exception($"Type {item.Type} not have constructor with not parameters");

                    il.Newobj(constr);
                    il.Stloc(in_value);


                    CompileReader(item.BinaryStruct, il, binaryStruct, buffer, offset, in_value, typeSize);

                    il.Ldloc(result);
                    il.Ldloc(in_value);
                    il.Call(item.Setter, isVirtual: true);

                    il.MarkLabel(methodBreak);
                }
            }
        }

        #endregion

        #region NULL

        /// <summary>
        /// Вставить фрагмент кода для записи Nullable типа NULL == true и переноса на указанную метку
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bs"></param>
        /// <param name="il"></param>
        /// <param name="finishMethod"></param>
        /// <param name="value"></param>
        /// <param name="buffer"></param>
        public static void WriteNullableType<T>(BinaryStruct bs, GroboIL il, GroboIL.Label finishMethod, GroboIL.Local value, GroboIL.Local buffer)
            where T : struct
        {
            il.Ldloca(value);
            WriteNullableType<T>(bs, il, finishMethod, buffer);
        }

        /// <summary>
        /// Вставить фрагмент кода для записи Nullable типа NULL == true и переноса на указанную метку
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bs"></param>
        /// <param name="il"></param>
        /// <param name="finishMethod"></param>
        /// <param name="buffer"></param>
        public static void WriteNullableType<T>(BinaryStruct bs, GroboIL il, GroboIL.Label finishMethod, GroboIL.Local buffer)
            where T : struct
        {
            var _null = il.DeclareLocal(typeof(bool));

            il.Call(typeof(Nullable<T>).GetProperty("HasValue").GetGetMethod());
            il.Ldc_I4(0);
            il.Ceq();
            il.Stloc(_null);

            il.ArrayByteSetter(buffer, _null);

            il.Ldloc(_null);
            il.Brtrue(finishMethod);
        }

        /// <summary>
        /// Вставить фрагмент кода для чтения NULL == true и переноса на указанную метку
        /// </summary>
        /// <param name="il"></param>
        /// <param name="finishMethod"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="typeSize"></param>
        public static void ReadNullableType(GroboIL il, GroboIL.Label finishMethod, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldelem(typeof(byte));
            WriteOffsetAppend(il, offset, 1);
            il.Brtrue(finishMethod);

        }

        /// <summary>
        /// Вставить фрагмент кода для записи NULL == true и переноса на указанную метку
        /// </summary>
        /// <param name="s"></param>
        /// <param name="il"></param>
        /// <param name="finishMethod"></param>
        /// <param name="value"></param>
        /// <param name="buffer"></param>
        /// <param name="typeSize"></param>
        public static void WriteObjectNull(BinaryStruct s, GroboIL il, GroboIL.Label finishMethod, GroboIL.Local value, GroboIL.Local buffer, GroboIL.Local typeSize)
        {
            il.Ldloc(value);
            WriteObjectNull(s, il, finishMethod, buffer, typeSize);
        }

        /// <summary>
        /// Вставить фрагмент кода для записи NULL == true и переноса на указанную метку
        /// </summary>
        /// <param name="s"></param>
        /// <param name="il"></param>
        /// <param name="finishMethod"></param>
        /// <param name="buffer"></param>
        /// <param name="typeSize"></param>
        public static void WriteObjectNull(BinaryStruct s, GroboIL il, GroboIL.Label finishMethod, GroboIL.Local buffer, GroboIL.Local typeSize)
        {
            var _null = il.DeclareLocal(typeof(bool));

            il.Ldnull();
            il.Ceq();
            il.Stloc(_null);


            var arr = s.TempBuildValues["tempBuffer"].Value;

            il.Ldloc(_null);
            il.Call(typeof(BitConverter).GetMethod("GetBytes", new Type[] { typeof(bool) }));
            il.Stloc(arr);

            il.ArraySetter(buffer, arr, 1);

            il.Ldloc(_null);
            il.Brtrue(finishMethod);
        }

        /// <summary>
        /// Вставить фрагмент кода для чтения NULL == true и переноса на указанную метку
        /// </summary>
        /// <param name="il"></param>
        /// <param name="finishMethod"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="typeSize"></param>
        public static void ReadObjectNull(GroboIL il, GroboIL.Label finishMethod, GroboIL.Local buffer, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            il.Ldloc(buffer);
            il.Ldloc(offset);
            il.Ldelem(typeof(byte));
            WriteOffsetAppend(il, offset, 1);
            il.Brtrue(finishMethod);

        }

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

        #region Offset

        /// <summary>
        /// Вставка фрагмента кода для добавления размера типа к позиции
        /// </summary>
        /// <param name="il"></param>
        /// <param name="offset"></param>
        /// <param name="typeSize"></param>
        public static void WriteOffsetAppend(GroboIL il, GroboIL.Local offset, GroboIL.Local typeSize)
        {
            il.Ldloc(offset);
            il.Ldloc(typeSize);
            il.Add();
            il.Stloc(offset);
        }

        /// <summary>
        /// Вставка фрагмента кода для добавления размера типа к позиции
        /// </summary>
        /// <param name="il"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        public static void WriteOffsetAppend(GroboIL il, GroboIL.Local offset, int len)
        {
            il.Ldloc(offset);
            il.Ldc_I4(len);
            il.Add();
            il.Stloc(offset);
        }

        #endregion

        internal static ConstructorInfo GetConstructor(Type type, Type[] args)
        {
            var construct = type.GetConstructors(BindingFlags.Public |
                 BindingFlags.NonPublic |
                 BindingFlags.Instance |
                 BindingFlags.DeclaredOnly);

            if (args == null)
                return construct.OrderBy(x => x.GetParameters().Count() != 0).First();

            return construct.First(x => x.GetParameters().Select(y => y.ParameterType).SequenceEqual(args));
        }
    }
}
