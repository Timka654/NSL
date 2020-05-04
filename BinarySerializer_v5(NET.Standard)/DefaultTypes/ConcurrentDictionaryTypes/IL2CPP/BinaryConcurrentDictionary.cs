using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public partial class BinaryConcurrentDictionary<TKey,TValue> : IBasicType
    {

#if !NOT_UNITY

        public object GetReadILCode(BinaryMemberData _prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            if (!BinaryStruct.ReadObjectNull(buffer))
                return _prop.Type.GetDefaultValue();

            int len = _prop.ArraySize;

            if (_prop.ArraySizeProperty != null)
                len = (int)_prop.ArraySizeProperty.Getter.Invoke(currentObject, new object[] { });

            Array arr = Array.CreateInstance(_prop.Type, len);

            if (_prop.IsBaseType)
            {
                for (int i = 0; i < len; i++)
                {
                    arr.SetValue(_prop.BinaryType.GetReadILCode(_prop, currentStruct, buffer, currentObject), i);
                }
            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    arr.SetValue(BinaryStruct.CompileReader(_prop.BinaryStruct, buffer), i);
                }
            }
            return arr;
        }

        public void GetWriteILCode(BinaryMemberData _prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            var value = (DictionaryBase)_prop.Getter.Invoke(currentObject, new object[] { });

            if (!BinaryStruct.WriteObjectNull(value, buffer))
                return;

            int len = _prop.ArraySize;

            if (_prop.ArraySizeProperty != null)
                len = (int)_prop.ArraySizeProperty.Getter.Invoke(value, new object[] { });

            bool baseKey = typeof(IBasicType).IsAssignableFrom(_prop.Type.GetGenericArguments()[0]);
            bool baseValue = typeof(IBasicType).IsAssignableFrom(_prop.Type.GetGenericArguments()[1]);

            var enumerator = value.GetEnumerator();


                for (int i = 0; i < len && enumerator.MoveNext(); i++)
            {

                if (baseKey)
                    _prop.BinaryType.GetWriteILCode(_prop, currentStruct, buffer, enumerator.Key);
                else
                    BinaryStruct.CompileWriter(_prop.BinaryStruct, enumerator.Key, buffer);

                if (baseKey)
                    _prop.BinaryType.GetWriteILCode(_prop, currentStruct, buffer, enumerator.Key);
                else
                    BinaryStruct.CompileWriter(_prop.BinaryStruct, enumerator.Key, buffer);
            }
            
        }
#endif
    }
}
