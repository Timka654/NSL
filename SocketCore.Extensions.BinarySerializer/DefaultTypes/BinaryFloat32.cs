﻿using BinarySerializer;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketCore.Extensions.BinarySerializer.DefaultTypes
{
    public class BinaryFloat32 : BinaryTypeBasic
    {
        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return (ms, s, obj) => ((InputPacketBuffer)ms).ReadFloat();
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            dynamic func = Extensions.CreateGetPropertyFuncDynamic<float>(property);

            return (ms, s, val) => ((OutputPacketBuffer)ms).WriteFloat32(func(val));
        }
    }
}
