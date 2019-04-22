using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using BinarySerializer.DefaultTypes;

namespace BinarySerializer
{
    public class TypeStorage
    {
        public static TypeStorage Instance { get; } = new TypeStorage();

        private ConcurrentDictionary<Type, ConcurrentDictionary<string, List<PropertyData>>> TypeCacheMap { get; set; }

        public TypeStorage()
        {
            TypeCacheMap = new ConcurrentDictionary<Type, ConcurrentDictionary<string, List<PropertyData>>>();
        }

        public List<PropertyData> GetTypeInfo(string schemeName, Type type, Dictionary<Type, BasicType> TypeInstanceMap)
        {
            if (!TypeCacheMap.ContainsKey(type) || !TypeCacheMap[type].ContainsKey(""))
            {
                TypeCacheMap.TryAdd(type, new ConcurrentDictionary<string, List<PropertyData>>());
                LoadTypeInfo(type, TypeInstanceMap);
            }

            if (!TypeCacheMap[type].ContainsKey(schemeName))
            {
                TypeCacheMap[type].TryAdd(schemeName, TypeCacheMap[type][""].Where(x =>
                x.Property.GetCustomAttributes<BinarySchemeAttribute>().Count(z => z.SchemeName == schemeName) > 0).ToList());
            }

            return TypeCacheMap[type][schemeName];
        }

        private void LoadTypeInfo(Type type, Dictionary<Type, BasicType> TypeInstanceMap)
        {
            List<PropertyData> t = GetProperties(type, TypeInstanceMap);

            TypeCacheMap[type].TryAdd("", t.ToList());

            foreach (var item in t)
            {
                item.FillSize(t);
                item.SetMethods(CreateSetter(type, item.Property), CreateGetter(type, item.Property));
                if (item.Attrib.Type.BaseType != typeof(BasicType))
                {
                    GetTypeInfo("", item.Attrib.Type, TypeInstanceMap);
                }
            }
        }

        private List<PropertyData> GetProperties(Type type,Dictionary<Type, BasicType> TypeInstanceMap)
        {
            var r = type.GetProperties(
                BindingFlags.Public | 
                BindingFlags.NonPublic | 
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly).Where(x => 
                Attribute.GetCustomAttribute(x, typeof(BinaryAttribute)) != null).Select(x =>
                    new PropertyData(x, TypeInstanceMap, x.GetCustomAttribute<BinaryAttribute>())).ToList();

            if (type.BaseType != typeof(Object))
                r.AddRange(GetProperties(type.BaseType, TypeInstanceMap));

            return r;
        }

        public static Action<object, object> CreateSetter(Type entity, PropertyInfo propertyInfo)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression parameter = Expression.Parameter(typeof(object), "param");

            var body = Expression.Call(Expression.Convert(instance, entity), propertyInfo.GetSetMethod(true), Expression.Convert(parameter, propertyInfo.PropertyType));
            var parameters = new ParameterExpression[] { instance, parameter };
            return (Action<object, object>)Expression.Lambda(body, parameters).Compile();
        }

        private static Func<object, object> CreateGetter(Type entity, PropertyInfo propertyInfo)
        {
            var param = Expression.Parameter(typeof(object), "e");
            
            Expression body = Expression.Convert(Expression.PropertyOrField(Expression.TypeAs(param, entity), propertyInfo.Name), typeof(object));
            var getterExpression = Expression.Lambda<Func<object, object>>(body, param);
            return getterExpression.Compile();
        }
    }
}
