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
        return Expression.Lambda(Expression.New(t)).Compile();
        }

        public static Func<T, RT> CreateGetPropertyFunc<T, RT>(PropertyInfo propertyInfo)
        {
            ParameterExpression p = Expression.Parameter(typeof(T));
            return Expression.Lambda<Func<T, RT>>(
                Expression.Convert(Expression.Property(p, propertyInfo), typeof(RT)),
                p
            ).Compile();
        }

        public static Action<object, object> CreateSetPropertyFunc(PropertyInfo pi)
        {
            //p=> ((pi.DeclaringType)p).<pi>=(pi.PropertyType)v

            var expParamPo = Expression.Parameter(typeof(object), "p");
            var expParamPc = Expression.Convert(expParamPo, pi.DeclaringType);

            var expParamV = Expression.Parameter(typeof(object), "v");
            var expParamVc = Expression.Convert(expParamV, pi.PropertyType);

            var expMma = Expression.Call(
                    expParamPc
                    , pi.GetSetMethod()
                    , expParamVc
                );

            var exp = Expression.Lambda<Action<object, object>>(expMma, expParamPo, expParamV);

            return exp.Compile();
        }

        private static readonly MethodInfo CreateGetPropertyMethodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateGetPropertyFunc), BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo GetInstanceMethodInfo = typeof(Extensions).GetMethod(nameof(Extensions.GetInstance), BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo CreateSetPropertyMethodInfo = typeof(Extensions).GetMethod(nameof(Extensions.CreateSetPropertyFunc), BindingFlags.Public | BindingFlags.Static);

        public static dynamic CreateGetPropertyFuncDynamic<T>(PropertyInfo property)
        {
            return property != null ? CreateGetPropertyMethodInfo.MakeGenericMethod(property.DeclaringType, typeof(T)).Invoke(null, new object[] { property }) : (Func<dynamic, dynamic>)((v) => v);
        }
        public static dynamic CreateSetPropertyFuncDynamic<T>(PropertyInfo property)
        {
            return property != null ? CreateSetPropertyMethodInfo.Invoke(null, new object[] { property }) : (Action<object, object>)((v,v2) => { });
        }
        public static dynamic CreateGetPropertyFuncDynamic(PropertyInfo property)
        {
            return property != null ? CreateGetPropertyMethodInfo.MakeGenericMethod(property.DeclaringType, property.PropertyType).Invoke(null, new object[] { property }) : (Func<dynamic, dynamic>)((v) => v);
        }
        public static dynamic CreateSetPropertyFuncDynamic(PropertyInfo property)
        {
            return property != null ? CreateSetPropertyMethodInfo.Invoke(null, new object[] { property }) : (Func<dynamic, dynamic>)((v) => v);
        }

        public static dynamic GetInstanceFuncDynamic(PropertyInfo property)
        {
            return property != null ? GetInstanceMethodInfo.MakeGenericMethod(property.PropertyType).Invoke(null, null) : (Func<dynamic>)(() => null);
        }
    }
}
