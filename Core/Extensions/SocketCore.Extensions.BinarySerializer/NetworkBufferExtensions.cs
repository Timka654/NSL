//using SocketCore.Utils.Buffer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SocketCore.Extensions.BinarySerializer
//{
//    public static class NetworkBufferExtensions
//    {
//        public static T Deserialize<T>(this BinaryNetworkStorage storage, InputPacketBuffer buffer, string scheme = "default")
//            where T : new()
//        {
//            if (!storage.ContainsType(typeof(T)))
//                storage.BuildType(typeof(T));

//            T result = storage.GetReadAction(typeof(T), null, scheme)(buffer, null, null);

//            return result;

//        }

//        public static void FillType<T>(this BinaryNetworkStorage storage) {

//            if (!storage.ContainsType(typeof(T)))
//                storage.BuildType(typeof(T));
//        }

//        public static void Serialize<T>(this BinaryNetworkStorage storage, OutputPacketBuffer buffer, T value, string scheme = "default")
//        {
//            if (!storage.ContainsType(typeof(T)))
//                storage.BuildType(typeof(T));

//            storage.GetWriteAction(typeof(T),null, scheme)(buffer,null,value);
//        }
//    }
//}
