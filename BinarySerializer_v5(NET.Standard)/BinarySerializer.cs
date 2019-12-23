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


        public T Deserialize<T>(string scheme, byte[] buffer, ref int offset, int initialSize = 32)
        {
            var type = storage.GetTypeInfo(typeof(T), scheme, initialSize);
            var result = type.ReadMethod(buffer, type, offset);

            offset = result.Item1;

            return (T)result.Item2;
        }

        public T Deserialize<T>(string scheme, byte[] buffer, int initialSize = 32)
        {
            var type = storage.GetTypeInfo(typeof(T), scheme,initialSize);
            var result = type.ReadMethod(buffer, type, 0);

            return (T)result.Item2;
        }

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
