using BinarySerializer.DefaultTypes;
using GrEmit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BinarySerializer
{
    public class BinarySerializer
    {
        private TypeStorage storage;

        public BinarySerializer(TypeStorage typeStorage)
        {
            storage = typeStorage;
        }

        public BinarySerializer() : this(TypeStorage.Instance)
        {

        }

        /// <summary>
        /// Дессериализация данных по указанной схеме, с указанной позиции и возвратом offset после завершения процесса
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scheme"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="initialSize"></param>
        /// <returns></returns>
        public T Deserialize<T>(string scheme, byte[] buffer, ref int offset, int initialSize = 32)
        {
            var type = storage.GetTypeInfo(typeof(T), scheme, initialSize);
            var result = type.ReadMethod(buffer, type, offset);

            offset = result.Item1;

            return (T)result.Item2;
        }

        /// <summary>
        /// Дессериализация данных по указанной схеме
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scheme"></param>
        /// <param name="buffer"></param>
        /// <param name="initialSize"></param>
        /// <returns></returns>
        public T Deserialize<T>(string scheme, byte[] buffer, int initialSize = 32)
        {
            var type = storage.GetTypeInfo(typeof(T), scheme,initialSize);
            var result = type.ReadMethod(buffer, type, 0);

            return (T)result.Item2;
        }

        /// <summary>
        /// Серриализация данных по указанной схеме
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scheme"></param>
        /// <param name="obj"></param>
        /// <param name="initialSize"></param>
        /// <returns></returns>
        public byte[] Serialize<T>(string scheme, T obj, int initialSize = 32)
        {
           var type = storage.GetTypeInfo(typeof(T), scheme, initialSize);
           var result = type.WriteMethod(obj, type);

            var buffer = result.Item2;

            if (result.Item1 != result.Item2.Length)
                Array.Resize(ref buffer, result.Item1);

            return buffer;
        }
    }
}
