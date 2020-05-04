using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public partial class BinaryNullable<T, TType> : IBasicType
        where T : IBasicType, new()
        where TType : struct
    {
#if !NOT_UNITY

        public object GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            if (BinaryStruct.ReadObjectNull(buffer))
                return null;

            return (new T()).GetReadILCode(prop, currentStruct, buffer,currentObject);
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            //if (BinaryStruct.WriteNullableType<T>(currentStruct, currentObject, buffer))
            //    (new T()).GetWriteILCode(prop, currentStruct, buffer, currentObject)

        }

#endif
    }
}
