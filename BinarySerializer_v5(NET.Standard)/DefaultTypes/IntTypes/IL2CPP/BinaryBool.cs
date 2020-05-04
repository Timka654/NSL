using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;
using GrEmit;
using GrEmit.Utils;
using System.Globalization;

namespace BinarySerializer.DefaultTypes
{
    public partial class BinaryBool : IBasicType
    {
#if !NOT_UNITY

        public object GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            return buffer.ReadByte() == 1;
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            var propert = (PropertyInfo)prop.MemberInfo;

            var obj = propert.GetValue(currentObject);

            //buffer.WriteByte((byte)(value ? 1 : 0));
        }

#endif
    }
}
