using NSL.Extensions.BinarySerializer;
using SocketCore.Utils.Buffer;
using System.Reflection;

namespace NSL.SocketCore.Extensions.BinarySerializer.DefaultTypes
{
    public class BinaryFloat64 : BinaryTypeBasic
    {
        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return (ms, s, obj) => ((InputPacketBuffer)ms).ReadDouble();
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            dynamic func = Extensions.CreateGetPropertyFuncDynamic<double>(property);

            return (ms, s, val) => ((OutputPacketBuffer)ms).WriteFloat64(func(val));
        }
    }
}
