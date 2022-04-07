using NSL.Extensions.BinarySerializer;
using SocketCore.Utils.Buffer;
using System.Reflection;

namespace NSL.SocketCore.Extensions.BinarySerializer.DefaultTypes
{
    public class BinaryUInt64 : BinaryTypeBasic
    {
        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return (ms, s, obj) => ((InputPacketBuffer)ms).ReadUInt64();
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            dynamic func = Extensions.CreateGetPropertyFuncDynamic<ulong>(property);

            return (ms, s, val) => ((OutputPacketBuffer)ms).WriteUInt64(func(val));
        }
    }
}
