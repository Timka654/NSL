﻿using BinarySerializer;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketCore.Extensions.BinarySerializer.DefaultTypes
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
