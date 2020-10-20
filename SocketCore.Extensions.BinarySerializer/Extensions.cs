using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketCore.Extensions.BinarySerializer
{
    internal class Extensions
    {
        public static Func<T> GetInstance<T>()
        {
            if (typeof(T).GetConstructor(new Type[] { }) == null)
                return null;

            List<ParameterExpression> prms = new List<ParameterExpression>();

           return Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }

        public static dynamic GetInstanceByType(Type t)
        {
            if (t.IsAbstract)
                throw new Exception($"Cannot build action for create object of type {t} because this type is abstract");
            return Expression.Lambda(Expression.New(t)).Compile();
        }

        public static Func<TObject, TProperty> CreateGetPropertyFunc<TObject, TProperty>(PropertyInfo propertyInfo)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject), "value");

            Expression propertyGetterExpression = Expression.Property(paramExpression, propertyInfo.Name);

            Func<TObject, TProperty> result =
                Expression.Lambda<Func<TObject, TProperty>>(propertyGetterExpression, paramExpression).Compile();

            return result;
        }

        public static Action<TObject, TProperty> CreateSetPropertyFunc<TObject, TProperty>(PropertyInfo pi)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject));

            ParameterExpression paramExpression2 = Expression.Parameter(typeof(TProperty), pi.Name);

            MemberExpression propertyGetterExpression = Expression.Property(paramExpression, pi.Name);

            Action<TObject, TProperty> result = Expression.Lambda<Action<TObject, TProperty>>
            (
                Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression, paramExpression2
            ).Compile();

            return result;
        }

        private static readonly MethodInfo CreateGetPropertyMethodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateGetPropertyFunc), BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo GetInstanceMethodInfo = typeof(Extensions).GetMethod(nameof(Extensions.GetInstance), BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo CreateSetPropertyMethodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateSetPropertyFunc), BindingFlags.Public | BindingFlags.Static);

        public static dynamic CreateGetPropertyFuncDynamic<T>(PropertyInfo property)
        {
            return property != null ? CreateGetPropertyMethodInfo.MakeGenericMethod(property.DeclaringType, property.PropertyType).Invoke(null, new object[] { property }) : (Func<dynamic, dynamic>)((v) => v);
        }

        public static dynamic CreateGetPropertyFuncDynamic(PropertyInfo property)
        {
            return property != null ? CreateGetPropertyMethodInfo.MakeGenericMethod(property.DeclaringType, property.PropertyType).Invoke(null, new object[] { property }) : (Func<dynamic, dynamic>)((v) => v);
        }

        public static dynamic CreateSetPropertyFuncDynamic<T>(PropertyInfo property)
        {
            return property != null ? CreateSetPropertyMethodInfo.MakeGenericMethod(property.DeclaringType,typeof(T)).Invoke(null, new object[] { property }) : (Action<object, object>)((v,v2) => { });
        }
        public static dynamic CreateSetPropertyFuncDynamic(PropertyInfo property)
        {
            return property != null ? CreateSetPropertyMethodInfo.MakeGenericMethod(property.DeclaringType, property.PropertyType).Invoke(null, new object[] { property }) : (Func<dynamic, dynamic>)((v) => v);
        }

        public static dynamic GetInstanceFuncDynamic(PropertyInfo property)
        {
            return property != null ? GetInstanceMethodInfo.MakeGenericMethod(property.PropertyType).Invoke(null, null) : (Func<dynamic>)(() => null);
        }
    }
}
