using BinarySerializer;
using SocketCore.Utils.Buffer;
using System;
using System.Reflection;

namespace NSL.SocketCore.Extensions.BinarySerializer.DefaultTypes
{
    public class BinaryTimeSpan : BinaryTypeBasic
    {
        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return (ms, s, val) => TimeSpan.FromMilliseconds(((InputPacketBuffer)ms).ReadDouble());
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            dynamic func = Extensions.CreateGetPropertyFuncDynamic<TimeSpan>(property);

            return (ms, s, val) => ((OutputPacketBuffer)ms).WriteFloat64(func(val).TotalMilliseconds);
        }
    }
}
