//using BinarySerializer;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace NSL.SocketCore.Extensions.BinarySerializer.IEnumerableTypes
//{
//    public class BinaryList : BinaryTypeBasic
//    {
//        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
//        {
//            var test = property.PropertyType.GetGenericArguments()[0];

//            if (!Storage.ContainsType(test, scheme))
//                ((BinaryNetworkStorage)Storage).BuildType(test);

//            var action = Storage.GetReadAction(test, null, scheme);

//            var lenAction = Storage.GetReadAction(typeof(int), null, scheme);

//            var getter = Extensions.GetInstanceFuncDynamic(property);

//            return (ms, s, obj) =>
//            {
//                int len = lenAction(ms, s, obj);

//                dynamic data = getter();

//                for (int i = 0; i < len; i++)
//                {
//                    data.Add(action(ms, s, obj));
//                }

//                return data;
//            };
//        }

//        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
//        {
//            var test = property.PropertyType.GetGenericArguments()[0];

//            if (!Storage.ContainsType(test, scheme))
//                ((BinaryNetworkStorage)Storage).BuildType(test);

//            var action = Storage.GetWriteAction(test, null, scheme);

//            var lenAction = Storage.GetWriteAction(typeof(int), null, scheme);

//            //base.Storage.GetWriteAction(property.PropertyType.GetGenericTypeDefinition)

//            return (ms, s, obj) => {

//                lenAction(ms, s, obj.Count);
//                foreach (dynamic item in obj)
//                {
//                    action(ms, s, item);
//                }
//            };
//        }
//    }
//}
