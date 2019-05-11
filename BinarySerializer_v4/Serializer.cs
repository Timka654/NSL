using BinarySerializer.DefaultTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinarySerializer
{
    public partial class BinarySerializer
    {
        internal object ProcessObject { get; set; }
        internal string SchemeName { get; set; }

        internal byte[] Buffer;
        public byte[] Serialize<T>(string schemeName, T value, int len = 32)
        {
            SchemeName = schemeName;
            ProcessObject = value;

            //buffer = new byte[len];
            Buffer = new byte[len];
            Offset = 0;

            
            Serialize(schemeName, value, TypeStorage.GetTypeInfo(schemeName, typeof(T), TypeInstanceMap));

            return Buffer;
        }

        internal byte[] Serialize(string schemeName,object value, int len = 32)
        {
            SchemeName = schemeName;
            ProcessObject = value;
            
            Serialize(schemeName,value, TypeStorage.GetTypeInfo(schemeName, value.GetType(), TypeInstanceMap));

            return Buffer;
        }

        private void Serialize(string schemeName, object value, List<PropertyData> props)
        {
            string s = "";

            foreach (var item in props)
            {
                CurrentProperty = item;
                s = $"{CurrentSerializedType} {CurrentProperty} -->\r\n";
                Stack += s;
                item.Serialize(this);
                Stack = Stack.Substring(0, Stack.Length - s.Length);
            }
        }
    }
}
