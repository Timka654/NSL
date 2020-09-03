using BinarySerializer;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketCore.Extensions.BinarySerializer.DefaultTypes
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
