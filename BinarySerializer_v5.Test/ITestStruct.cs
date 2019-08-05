using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
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

        protected T deserializedValue;

        protected T binaryWritedValue;

        protected T binaryReadedValue;

        protected T normalValue;

        public virtual void bsSerializeAction(Stopwatch sw)
        {
            var r = GetRandomValue();

            if (serializerWriteBuffer == null)
            {
                serializedValue = r;
                serializerWriteBuffer = bs.Serialize("default", r);
            }
            sw.Start();
            bs.Serialize("default", r);
            sw.Stop();
        }

        public virtual void bsDesserializeAction(Stopwatch sw)
        {
            int offset = 0;
            if (deserializedValue == null)
            {
                deserializedValue = bs.Deserialize<T>("default", serializerWriteBuffer,ref offset);
                offset = 0;
            }

            sw.Start();
            bs.Deserialize<T>("default", serializerWriteBuffer, ref offset);
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
            bool compareSerializerValues = CompareSerializerValues();

            bool compareStreamValues = CompareStreamValues();

            bool compareSerializerBuffers = CompareSerializerBuffers();

            bool compareStreamBuffers = CompareStreamBuffers();

            if ((compareSerializerValues & compareStreamValues & compareSerializerBuffers & compareStreamBuffers) == false)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"Compare serialize values: {compareSerializerValues}");

            Console.WriteLine($"Compare stream values: {compareStreamValues}");

            Console.WriteLine($"Compare serialize buffers: {compareSerializerBuffers}");

            Console.WriteLine($"Compare stream buffers: {compareStreamBuffers}");

            Console.WriteLine($"Compare result: {compareSerializerValues & compareStreamValues & compareSerializerBuffers & compareStreamBuffers}");

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private bool CompareSerializerValues()
        {
            return ComparePropertyType(typeof(T), serializedValue, deserializedValue);
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
            else if (typeof(IDictionary).IsAssignableFrom(item.PropertyType))
            {
                var v = value1 as IDictionary;
                var v1 = value2 as IDictionary;

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

                var typeKey = item.PropertyType.GetGenericArguments()[0];
                var typeValue = item.PropertyType.GetGenericArguments()[1];

                var v1KeyEnumerator = v.Keys.GetEnumerator();
                var v2KeyEnumerator = v1.Keys.GetEnumerator();

                var v1ValueEnumerator = v.Values.GetEnumerator();
                var v2ValueEnumerator = v1.Values.GetEnumerator();

                for (int i = 0; i < v1.Count; i++)
                {
                    v1KeyEnumerator.MoveNext();
                    v2KeyEnumerator.MoveNext();
                    v1ValueEnumerator.MoveNext();
                    v2ValueEnumerator.MoveNext();

                    if (typeof(string).IsAssignableFrom(typeKey) || typeKey.IsPrimitive)
                    {
                        if (!CompareItem(typeKey, v1KeyEnumerator.Current, v2KeyEnumerator.Current))
                            return false;
                    }
                    else if (!ComparePropertyType(typeKey, v1KeyEnumerator.Current, v2KeyEnumerator.Current))
                        return false;

                    if (typeof(string).IsAssignableFrom(typeValue) || typeValue.IsPrimitive)
                    {

                        if (!CompareItem(typeValue, v1ValueEnumerator.Current, v2ValueEnumerator.Current))
                            return false;
                    }
                    else if (!ComparePropertyType(typeValue, v1ValueEnumerator.Current, v2ValueEnumerator.Current))
                        return false;

                }

                return true;
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
                return true;
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

                    if (!ComparePropertyType(type, v[i], v1[i]))
                        return false;
                }
                return true;
            }
            else if (typeof(Vector2).IsAssignableFrom(item.PropertyType))
            {
                var v = ((Vector2)value1);
                var v1 = ((Vector2)value2);

                if (v.X != v1.X || v.Y != v1.Y)
                    return false;

                return true;
            }
            else if (typeof(Vector3).IsAssignableFrom(item.PropertyType))
            {
                var v = ((Vector3)value1);
                var v1 = ((Vector3)value2);

                if (v.X != v1.X || v.Y != v1.Y || v.Z != v1.Z)
                    return false;

                return true;
            }
            else if (typeof(DateTime).IsAssignableFrom(item.PropertyType))
            {
                var v = ((DateTime)value1);
                var v1 = ((DateTime)value2);


                if (v.Year != v1.Year || v.Month != v1.Month || v.Day != v1.Day || v.Hour != v1.Hour || v.Minute != v1.Minute || v.Second != v1.Second || v.Millisecond != v1.Millisecond || v.Ticks != v1.Ticks)
                    return false;

                return true;
            }

            return false;
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
                return true;
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
