//using BinarySerializer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace NSL.SocketCore.Extensions.BinarySerializer.IEnumerableTypes
//{
//    public class BinaryDictionary : BinaryTypeBasic
//    {
//        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
//        {
//            var args = property.PropertyType.GetGenericArguments();

//            var keyType = args[0];

//            var valueType = args[1];

//            if (!Storage.ContainsType(keyType, scheme))
//                ((BinaryNetworkStorage)Storage).BuildType(keyType, scheme);

//#if DEBUG
//            string valueTypeName = valueType.FullName;
//#endif

//            if (!Storage.ContainsType(valueType, scheme))
//                ((BinaryNetworkStorage)Storage).BuildType(valueType,scheme);

//            var keyAction = Storage.GetReadAction(keyType, null, scheme);

//            var valueAction = Storage.GetReadAction(valueType, null, scheme);

//            var lenAction = Storage.GetReadAction(typeof(int), null, scheme);

//            var getter = Extensions.GetInstanceFuncDynamic(property);

//            return (ms, s, val) => {

//                int len = lenAction(ms, s, val);

//                var data = getter();

//                for (int i = 0; i < len; i++)
//                {
//                    data.Add(keyAction(ms, s, data), valueAction(ms, s, data));
//                }

//                return data;
//            };

//        }

//        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
//        {
//            var args = property.PropertyType.GetGenericArguments();

//            var keyType = args[0];

//            var valueType = args[1];

//            if (!Storage.ContainsType(keyType, scheme))
//                ((BinaryNetworkStorage)Storage).BuildType(keyType, scheme);

//            if (!Storage.ContainsType(valueType, scheme))
//                ((BinaryNetworkStorage)Storage).BuildType(valueType, scheme);


//            var keyAction = Storage.GetWriteAction(keyType, null, scheme);

//            var valueAction = Storage.GetWriteAction(valueType, null, scheme);

//            var lenAction = Storage.GetWriteAction(typeof(int), null, scheme);
//            return (ms, s, val) => {

//                lenAction(ms, s, val.Count);
//                foreach (var item in val)
//                {
//                    keyAction(ms, s, item.Key);
//                    valueAction(ms, s, item.Value);
//                }
//            };
//        }
//    }
//}
