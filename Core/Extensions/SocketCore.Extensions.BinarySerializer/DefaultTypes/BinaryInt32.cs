﻿using NSL.Extensions.BinarySerializer;
using NSL.SocketCore.Utils.Buffer;
using System.Reflection;

namespace NSL.SocketCore.Extensions.BinarySerializer.DefaultTypes
{
    public class BinaryInt32 : BinaryTypeBasic
    {
        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return (ms, s, obj) =>  ((InputPacketBuffer)ms).ReadInt32();
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            dynamic func = Extensions.CreateGetPropertyFuncDynamic<int>(property);

            return (ms, s, val) => ((OutputPacketBuffer)ms).WriteInt32(func(val));
        }


    }

}