using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace BinarySerializer_v5.Test
{
    public class ITestStruct<T>
    {
        protected BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

        protected byte[] serializerWriteBuffer;

        protected byte[] streamWriteBuffer;

        protected T serializedValue;

        protected T desserializedValue;

        protected T binaryWritedValue;

        protected T binaryReadedValue;

        protected T normalValue;

        public virtual void bsSerializeAction(Stopwatch sw)
        {
            var r = GetRandomValue();

            if (serializerWriteBuffer == null)
            {
                serializedValue = r;
                serializerWriteBuffer = bs.Serialize(r, "");
            }
            sw.Start();
            bs.Serialize(r, "");
            sw.Stop();
        }

        public virtual void bsDesserializeAction(Stopwatch sw)
        {
            if (desserializedValue == null)
            {
                desserializedValue = bs.Desserialize<T>(serializerWriteBuffer, "");
            }

            sw.Start();
            bs.Desserialize<T>(serializerWriteBuffer, "");
            sw.Stop();
        }

        public virtual void streamWriteFunc(Stopwatch sw)
        {

        }

        public virtual void streamReadFunc(Stopwatch sw)
        {

        }

        public virtual T GetRandomValue()
        {
            return default(T);
        }

        public virtual void Compare()
        {
            bool wbr = CompareStreamBuffers();

            bool compareSerializerValues = CompareSerializerValues();

            bool compareStreamValues = CompareStreamValues();


            bool compareSerializerBuffers = CompareSerializerBuffers();

            bool compareStreamBuffers = CompareStreamBuffers();

            Console.WriteLine($"Compare serialize values: {compareSerializerValues}");

            Console.WriteLine($"Compare stream values: {compareStreamValues}");

            Console.WriteLine($"Compare serialize buffers: {compareSerializerBuffers}");

            Console.WriteLine($"Compare stream buffers: {compareStreamBuffers}");

            Console.WriteLine($"Compare result: {compareSerializerValues & compareStreamValues & compareSerializerBuffers & compareStreamBuffers}");

            //Console.WriteLine($"Serialize/Write buffer compare: {sbsb}");
        }

        private bool CompareSerializerValues()
        {
            return ComparePropertyType(typeof(T), serializedValue, desserializedValue);
        }

        private bool ComparePropertyType(Type prop, object value1, object value2)
        {
            var props = prop.GetProperties();

            foreach (var item in props)
            {
                if (!CompareProperty(item, item.GetValue(value1, new object[] { }), item.GetValue(value2, new object[] { })))
                    return false;
            }
            return true;
        }

        private bool CompareProperty(PropertyInfo item, object value1, object value2)
        {
            if (typeof(string).IsAssignableFrom(item.PropertyType) || item.PropertyType.IsPrimitive)
            {
                return CompareItem(item.PropertyType, value1, value2);
            }
            else if (typeof(Array).IsAssignableFrom(item.PropertyType))
            {
                var v = (value1 as Array);
                var v1 = (value2 as Array);

                if (v == null && v1 == null)
                {
                    return true;
                }
                else if (v == null || v1 == null)
                {
                    return false;
                }

                if (v1.Length != v.Length)
                    return false;

                var type = item.PropertyType.GetElementType();

                for (int i = 0; i < v.Length; i++)
                {
                    if (typeof(string).IsAssignableFrom(type) || type.IsPrimitive)
                    {

                        if (!CompareItem(type, v.GetValue(i), v1.GetValue(i)))
                            return false;
                        continue;
                    }

                    if (!ComparePropertyType(type, v.GetValue(i), v1.GetValue(i)))
                        return false;
                }
            }
            else if (typeof(IList).IsAssignableFrom(item.PropertyType))
            {
                var v = ((IList)value1);
                var v1 = ((IList)value2);

                if (v == null && v1 == null)
                {
                    return true;
                }
                else if (v == null || v1 == null)
                {
                    return false;
                }

                if (v1.Count != v.Count)
                    return false;

                var type = item.PropertyType.GetGenericArguments()[0];

                for (int i = 0; i < v.Count; i++)
                {
                    if (typeof(string).IsAssignableFrom(type) || type.IsPrimitive)
                    {

                        if (!CompareItem(type, v[i], v1[i]))
                            return false;
                        continue;
                    }

                    if (!ComparePropertyType(type, item.GetValue(value1, new object[] { i }), item.GetValue(value2, new object[] { i })))
                        return false;
                }
            }

            return true;
        }

        private bool CompareItem(Type item, object value1, object value2)
        {
            if (typeof(string).IsAssignableFrom(item))
            {
                var v = ((string)value1);
                var v1 = ((string)value2);

                if (v == null && v1 == null)
                {
                    return true;
                }
                else if (v == null || v1 == null)
                {
                    return false;
                }

                if (v.Length != v1.Length || v != v1)
                    return false;
            }
            else if (value1?.Equals(value2) != true)
                return false;

            return true;
        }


        private bool CompareStreamValues()
        {
            return ComparePropertyType(typeof(T), binaryWritedValue, binaryReadedValue);
        }

        private bool CompareSerializerBuffers()
        {

            return true;
        }


        private bool CompareStreamBuffers()
        {
            if (serializerWriteBuffer?.Length != streamWriteBuffer?.Length)
            {
                Console.WriteLine("Invalid length");
                return false;
            }

            for (int i = 0; i < serializerWriteBuffer.Length; i++)
            {
                if (serializerWriteBuffer[i] != streamWriteBuffer[i])
                    return false;
            }

            return true;
        }
    }
}
