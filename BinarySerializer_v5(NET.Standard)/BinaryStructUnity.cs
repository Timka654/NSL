using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine.Rendering;

namespace BinarySerializer
{
    public partial class BinaryStruct
    {
#if !NOT_UNITY

        #region Writer

        /// <summary>
        /// Скомпилировать метод для записи
        /// </summary>
        private void CompileWriter()
        {
            WriteMethod = new Func<object, BinaryStruct, Tuple<int, byte[]>>((obj, _struct) =>
            {
                var buffer = new IL2CPPMemoryStream(InitLen);

                var value = obj;

                CompileWriter(this, value, buffer);

                return new Tuple<int, byte[]>((int)buffer.Length, buffer.ToArray());
            });
#if DEBUG
            IlWriteCode = "";
#endif
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
        public static void CompileWriter(BinaryStruct bs, object value, IL2CPPMemoryStream buffer)
        {
            if (bs.PropertyList.Count > 0)
            {
                foreach (var item in bs.PropertyList)
                {
                    if (item.Setter == null)
                        continue;
                    ProcessWrite(item, bs,  value, buffer);
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
        public static void ProcessWrite(BinaryMemberData item, BinaryStruct bs, object value, IL2CPPMemoryStream buffer)
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
                item.BinaryType.GetWriteILCode(item, bs, buffer,value);
                return;
            }

            var in_value = item.Getter.Invoke(value, new object[] { });

            if(WriteObjectNull(bs, buffer))
                CompileWriter(item.BinaryStruct, in_value, buffer);
        }

        #endregion

        #region Reader

        /// <summary>
        /// Скомпилировать метод для чтения
        /// </summary>
        private void CompileReader()
        {
            ReadMethod = new Func<byte[], BinaryStruct, int, Tuple<int, object>>((buffer, stru, offset) => {

                IL2CPPMemoryStream ms = new IL2CPPMemoryStream(buffer);
                ms.Position = offset;

                object result = CompileReader(this, ms);

                return new Tuple<int, object>((int)ms.Position, result);
            });


#if DEBUG
            IlReadCode = "";
#endif
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
        public static object CompileReader(BinaryStruct bs, IL2CPPMemoryStream buffer)
        {
            object result = Activator.CreateInstance(bs.Type);

            if (bs.PropertyList.Count > 0)
            {
                foreach (var item in bs.PropertyList)
                {
                    if (item.Getter == null)
                        continue;

                    if (item.IsBaseType)
                    {
                        item.Setter.Invoke(result, new object[] { item.BinaryType.GetReadILCode(item, bs, buffer, result) });
                        continue;
                    }
                    if (ReadObjectNull(buffer))
                    {
                        item.Setter.Invoke(result, new object[] { CompileReader(item.BinaryStruct, buffer) });
                    }
                }
            }

            return result;
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
        /// <param name="buffer"></param>
        public static bool WriteNullableType<T>(BinaryStruct bs, object value, IL2CPPMemoryStream buffer)
            where T : struct
        {
            if (((Nullable<T>)value).HasValue)
            {
                buffer.WriteByte(0);
                return true;
            }
            else
            {
                buffer.WriteByte(1);
                return false;
            }
        }

        /// <summary>
        /// Вставить фрагмент кода для чтения NULL == true и переноса на указанную метку
        /// </summary>
        /// <param name="il"></param>
        /// <param name="finishMethod"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="typeSize"></param>
        public static bool ReadNullableType(IL2CPPMemoryStream buffer)
        {
            if (buffer.ReadByte() == 1)
                return false;

            return true;
        }

        /// <summary>
        /// Вставить фрагмент кода для записи NULL == true и переноса на указанную метку
        /// </summary>
        /// <param name="s"></param>
        /// <param name="il"></param>
        /// <param name="finishMethod"></param>
        /// <param name="buffer"></param>
        /// <param name="typeSize"></param>
        public static bool WriteObjectNull(object value, IL2CPPMemoryStream buffer)
        {
            if (value == null)
            {
                buffer.WriteByte(1);
                return false;
            }
            else
            {
                buffer.WriteByte(0);
                return true;
            }
        }

        /// <summary>
        /// Вставить фрагмент кода для чтения NULL == true и переноса на указанную метку
        /// </summary>
        /// <param name="il"></param>
        /// <param name="finishMethod"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="typeSize"></param>
        public static bool ReadObjectNull(IL2CPPMemoryStream buffer)
        {
            if (buffer.ReadByte() == 1)
                return false;
            else
                return true;
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

#endif
    }
}
