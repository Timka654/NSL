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

            foreach (var item in props)
            {
                item.Deserialize(this);
            }

            return ProcessObject;
        }
    }
}
