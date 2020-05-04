using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public partial class BinaryArray32<T> : IBasicType
    {
#if !NOT_UNITY

        public object GetReadILCode(BinaryMemberData _prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            if (!BinaryStruct.ReadObjectNull(buffer))
                return _prop.Type.GetDefaultValue();

            int len = buffer.ReadInt32();

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

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            Array value = (Array)prop.Getter.Invoke(currentObject, new object[] { });

            if (!BinaryStruct.WriteObjectNull(value, buffer))
                return;

            buffer.WriteInt32(value.Length);

            if (prop.IsBaseType)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    prop.BinaryType.GetWriteILCode(prop, currentStruct, buffer, value.GetValue(i));
                }
            }
            else
            {
                for (int i = 0; i < value.Length; i++)
                {
                    BinaryStruct.CompileWriter(prop.BinaryStruct, value.GetValue(i), buffer);
                }
            }
        }
#endif
    }
}
