using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;
using GrEmit;
using GrEmit.Utils;
#if NOT_UNITY
using System.Numerics;
#else
using UnityEngine;
#endif

namespace BinarySerializer.DefaultTypes
{
    public partial class BinaryVector3 : IBasicType
    {
#if !NOT_UNITY
        public object GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            return new Vector3(buffer.ReadFloat32(), buffer.ReadFloat32(), buffer.ReadFloat32());
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            var v = (Vector3)currentObject;

            buffer.WriteFloat32(v.x);
            buffer.WriteFloat32(v.y);
            buffer.WriteFloat32(v.z);
        }

#endif
    }
}
