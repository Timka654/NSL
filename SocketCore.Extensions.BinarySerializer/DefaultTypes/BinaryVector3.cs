//using BinarySerializer;
//using SocketCore.Utils.Buffer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//#if (Unity)
//using UnityEngine;
//#endif

//namespace SocketCore.Extensions.BinarySerializer.DefaultTypes
//{
//    public class BinaryVector3 : BinaryTypeBasic
//    {
//        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
//        {
//            return (ms, s, val) => new Vector3(((InputPacketBuffer)ms).ReadFloat(), ((InputPacketBuffer)ms).ReadFloat(), ((InputPacketBuffer)ms).ReadFloat());
//        }

//        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
//        {
//            dynamic func = Extensions.CreateGetPropertyFuncDynamic<Vector3>(property);

//            return (ms, s, val) =>
//            {
//                var val2 = func(val);
//                ((OutputPacketBuffer)ms).WriteFloat32(val2.x);
//                ((OutputPacketBuffer)ms).WriteFloat32(val2.y);
//                ((OutputPacketBuffer)ms).WriteFloat32(val2.z);
//            };
//        }
//    }
//}
