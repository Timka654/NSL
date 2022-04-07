using BinarySerializer;
using SocketCore.Utils.Buffer;
using System.Reflection;

namespace NSL.SocketCore.Extensions.BinarySerializer.DefaultTypes
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
