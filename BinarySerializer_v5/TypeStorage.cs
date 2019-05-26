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

        public TypeStorage(Encoding coding)
        {
            TypeCacheMap = new ConcurrentDictionary<Type, ConcurrentDictionary<string, BinaryStruct>>();
            Coding = coding;
        }
        
        public BinaryStruct GetTypeInfo(Type type, string schemeName, int initialSize = 32)
        {
            if (!TypeCacheMap.ContainsKey(type) || !TypeCacheMap[type].ContainsKey(""))
            {
                TypeCacheMap.TryAdd(type, new ConcurrentDictionary<string, BinaryStruct>());
                LoadType(type,initialSize);
            }

            if (!TypeCacheMap[type].ContainsKey(schemeName))
            {
                TypeCacheMap[type].TryAdd(schemeName, TypeCacheMap[type][""].GetSchemeData(schemeName, Coding, this));
            }

            return TypeCacheMap[type][schemeName];
        }

        private void LoadType(Type type, int initialSize )
        {
            List<PropertyData> t = GetProperties(type);
            var s = new BinaryStruct(type, "", t.ToList(), Coding, this);
            TypeCacheMap[type].TryAdd("", s);
            s.InitLen = initialSize;

            foreach (var item in t)
            {
                if (!item.IsBaseType)
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
                    new PropertyData(x)).ToList();

            //все наследуемые классы
            if (type.BaseType != typeof(Object))
                r.AddRange(GetProperties(type.BaseType));

            return r;
        }

        public void PreCompileBinaryStructs(Assembly assembly)
        {
            var classes = assembly.GetTypes().Select(x => new { x, attr = x.GetCustomAttribute<BinaryPreCompileAttribute>() }).Where(x => x.attr != null);

            foreach (var item in classes)
            {
                foreach (var scheme in item.attr.Schemes)
                {
                    GetTypeInfo(item.x, scheme, item.attr.InitialSize);
                }
            }
        }

        public void SetCoding(Encoding coding)
        {
            Coding = coding;
        }
    }
}
