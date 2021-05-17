//using BinarySerializer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace SocketCore.Extensions.BinarySerializer.IEnumerableTypes
//{
//    public class BinaryArray : BinaryTypeBasic
//    {
//        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
//        {
//            var test = property.PropertyType.GetElementType();

//            if (!Storage.ContainsType(test))
//                ((BinaryNetworkStorage)Storage).BuildType(test);

//            var action = Storage.GetReadAction(test, null, scheme);

//            var lenAction = Storage.GetReadAction(typeof(int), null, scheme);

//            Func<int,Array> getter = (int len) => Array.CreateInstance(test, len);

//            return (ms, s, obj) =>
//            {
//                int len = lenAction(ms, s, obj);

//                Array data = getter(len);
//                for (int i = 0; i < len; i++)
//                {
//                    data.SetValue(action(ms, s, obj),i);
//                }

//                return data;
//            };
//        }

//        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
//        {
//            var test = property.PropertyType.GetElementType();

//            if (!Storage.ContainsType(test))
//                ((BinaryNetworkStorage)Storage).BuildType(test);

//            var action = Storage.GetWriteAction(test, null, scheme);

//            var lenAction = Storage.GetWriteAction(typeof(int), null, scheme);

//            //base.Storage.GetWriteAction(property.PropertyType.GetGenericTypeDefinition)

//            return (ms, s, obj) => {

//                lenAction(ms, s, obj.Length);
//                foreach (dynamic item in obj)
//                {
//                    action(ms, s, item);
//                }
//            };
//        }
//    }
//}
