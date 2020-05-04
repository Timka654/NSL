using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;
using GrEmit;
using GrEmit.Utils;

namespace BinarySerializer.DefaultTypes
{
    public partial class BinaryTimeSpan : IBasicType
    {
#if !NOT_UNITY

        public object GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            return TimeSpan.FromMilliseconds(buffer.ReadFloat64());
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            buffer.WriteFloat64(((TimeSpan)currentObject).TotalMilliseconds);
        }

#endif

    }
}
