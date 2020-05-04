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
    public partial class BinaryInt16 : IBasicType
    {
#if !NOT_UNITY

        public object GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            return buffer.ReadInt16();
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            buffer.WriteInt16((short)currentObject);
        }

#endif

    }
}
