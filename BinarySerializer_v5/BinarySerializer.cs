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


        public T Desserialize<T>(byte[] buffer, string scheme = "", int offset = 0)
        {
            var type = storage.GetTypeInfo(typeof(T), scheme);
            var result = type.ReadMethod(buffer,type, offset);

            return (T)result;
        }

        public byte[] Serialize<T>(T obj, string scheme = "")
        {
           var type = storage.GetTypeInfo(typeof(T), scheme);
           var result = type.WriteMethod(obj, type);

            var buffer = result.Item2;

            if (result.Item1 != result.Item2.Length)
                Array.Resize(ref buffer, result.Item1);

            return buffer;
        }

    }
}
