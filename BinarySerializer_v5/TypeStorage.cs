using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace BinarySerializer
{
    public class TypeStorage
    {
        public static TypeStorage Instance { get; } = new TypeStorage(UTF8Encoding.UTF8);

        private ConcurrentDictionary<Type, ConcurrentDictionary<string, BinaryStruct>> TypeCacheMap { get; set; }

        private Encoding Coding { get; set; }

        private Dictionary<Type, IBasicType> TypeInstanceMap = new Dictionary<Type, IBasicType>()
            {
                //{ typeof(BinaryByte), new BinaryByte() },
                //{ typeof(BinaryBool), new BinaryBool() },
                //{ typeof(BinaryInt16), new BinaryInt16() },
                //{ typeof(BinaryUInt16), new BinaryUInt16() },
                { typeof(BinaryInt32), new BinaryInt32() },
                //{ typeof(BinaryUInt32), new BinaryUInt32() },
                //{ typeof(BinaryInt64), new BinaryInt64() },
                //{ typeof(BinaryUInt64), new BinaryUInt64() },
                //{ typeof(BinaryFloat32), new BinaryFloat32() },
                //{ typeof(BinaryFloat64), new BinaryFloat64() },
                //{ typeof(BinaryString), new BinaryString() },
                //{ typeof(BinaryString16), new BinaryString16() },
                { typeof(BinaryString32), new BinaryString32() },
                //{ typeof(BinaryDateTime), new BinaryDateTime() },
                //{ typeof(BinaryTimeSpan), new BinaryTimeSpan() },
                //{ typeof(BinaryVector2), new BinaryVector2() },
                //{ typeof(BinaryVector3), new BinaryVector3() },
            };

        public TypeStorage(Encoding coding)
        {
            TypeCacheMap = new ConcurrentDictionary<Type, ConcurrentDictionary<string, BinaryStruct>>();
            Coding = coding;
        }
        
        public BinaryStruct GetTypeInfo(Type type, string schemeName)
        {
            if (!TypeCacheMap.ContainsKey(type) || !TypeCacheMap[type].ContainsKey(""))
            {
                TypeCacheMap.TryAdd(type, new ConcurrentDictionary<string, BinaryStruct>());
                LoadType(type);
            }

            if (!TypeCacheMap[type].ContainsKey(schemeName))
            {
                TypeCacheMap[type].TryAdd(schemeName, TypeCacheMap[type][""].GetSchemeData(schemeName,Coding, this));
            }

            return TypeCacheMap[type][schemeName];
        }

        private void LoadType(Type type)
        {
            List<PropertyData> t = GetProperties(type);
            var s = new BinaryStruct(type, "", t.ToList(), Coding, this);
            TypeCacheMap[type].TryAdd("", s);

            foreach (var item in t)
            {
                if (!typeof(IBasicType).IsAssignableFrom(item.BinaryAttr.Type))
                {
                    item.BinaryStruct = GetTypeInfo(item.BinaryAttr.Type, "");
                }
            }
            s.Compile();
        }

        private List<PropertyData> GetProperties(Type type)
        {
            var r = type.GetProperties(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly).Where(x =>
                Attribute.GetCustomAttribute(x, typeof(BinaryAttribute)) != null).Select(x =>
                    new PropertyData(x,TypeInstanceMap)).ToList();

            if (type.BaseType != typeof(Object))
                r.AddRange(GetProperties(type.BaseType));

            return r;
        }

        private void LoadAllTypes()
        {

        }
    }
}
