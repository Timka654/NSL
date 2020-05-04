using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public partial class BinaryString16 : IBasicType
    {
#if !NOT_UNITY

        public object GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            return buffer.ReadString16();
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            var value = (string)prop.Getter.Invoke(currentObject, null);

            buffer.WriteString16(value);
        }

#endif
    }
}
