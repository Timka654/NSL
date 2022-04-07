using BinarySerializer;
using System.Linq;
using System.Reflection;

namespace NSL.SocketCore.Extensions.BinarySerializer.DefaultTypes
{
    public class BinaryNullable : BinaryTypeBasic
    {
        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            var action = Storage.GetReadAction(property.PropertyType.GetGenericArguments().First(), null, scheme);

            return (ms,s,obj)=> action(ms,s, obj); // (ms) => ((InputPacketBuffer)ms).ReadBool();
        }


        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            var action = Storage.GetWriteAction(property.PropertyType.GetGenericArguments().First(),null,scheme);

            return (ms,s, val) => action(ms, s,val); // (ms, val) => ((OutputPacketBuffer)ms).WriteBool(val);
        }
    }
}
