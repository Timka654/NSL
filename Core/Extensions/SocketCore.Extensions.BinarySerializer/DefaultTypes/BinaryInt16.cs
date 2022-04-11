using NSL.Extensions.BinarySerializer;
using SocketCore.Utils.Buffer;
using System.Reflection;

namespace NSL.SocketCore.Extensions.BinarySerializer.DefaultTypes
{
    public class BinaryInt16 : BinaryTypeBasic
    {
        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return (ms, s, obj) => ((InputPacketBuffer)ms).ReadInt16();
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            dynamic func = Extensions.CreateGetPropertyFuncDynamic<short>(property);

            return (ms, s, val) => ((OutputPacketBuffer)ms).WriteInt16(func(val));
        }
    }
}
