using NSL.Extensions.BinarySerializer;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Reflection;

namespace NSL.SocketCore.Extensions.BinarySerializer.DefaultTypes
{
    public class BinaryDateTime : BinaryTypeBasic
    {
        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return (ms, s, val) => ((InputPacketBuffer)ms).ReadDateTime();
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            dynamic func = Extensions.CreateGetPropertyFuncDynamic<DateTime>(property);

            return (ms, s, val) => ((OutputPacketBuffer)ms).WriteDateTime(func(val));
        }
    }
}
