using BinarySerializer.DefaultTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BinarySerializer
{
    public partial class BinarySerializer
    {
        public T Deserialize<T>(string schemeName, byte[] buffer, int offset = 0)
        {
            this.Buffer = buffer;
            this.Offset = offset;

            return (T)Deserialize(schemeName, typeof(T), TypeStorage.GetTypeInfo(schemeName, typeof(T), TypeInstanceMap));
        }

        internal object Deserialize(string schemeName, Type type, List<PropertyData> props)
        {
            SchemeName = schemeName;

            ProcessObject = Activator.CreateInstance(type);

            string s = "";

            foreach (var item in props)
            {
                CurrentProperty = item;
                s = $"{CurrentSerializedType} {CurrentProperty} -->\r\n";
                Stack += s;
                item.Deserialize(this);
                Stack = Stack.Substring(0, Stack.Length - s.Length);
            }

            return ProcessObject;
        }
    }
}
