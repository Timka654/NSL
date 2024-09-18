using NSL.Database.EntityFramework.Filter.Enums;
using NSL.Database.EntityFramework.Filter.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public class DBSetFilter<T>
        where T : class
    {
        public static DBSetFilteredResult<T> Filter(IQueryable<T> set, EntityFilterQueryModel filterExt)
        {
            IQueryable<T> query = set;

            if (filterExt?.FilterQuery?.Count > 0)
            {
                var li = Expression.Parameter(typeof(T), "p");

                Expression where = null;

                foreach (var block in filterExt.FilterQuery)
                {
                    Expression whereBlock = null;
                    foreach (var p in block.Properties)
                    {
                        BuildPropertyFilter(li, p, ref whereBlock);
                    }

                    Or(whereBlock, ref where);
                }

                var predicate = Expression.Lambda<Func<T, bool>>(where, new ParameterExpression[] { li });
                query = query.Where(predicate);
            }

            if (filterExt?.OrderQuery?.Count > 0)
            {
                var li = Expression.Parameter(typeof(T), "p");

                foreach (var block in filterExt.OrderQuery)
                {
                    bool first = true;
                    foreach (var p in block.Properties)
                    {
                        BuildPropertyOrder(li, p, ref query, first);
                        first = false;
                    }
                }
            }

            var result = new DBSetFilteredResult<T>() { CountQuery = query };

            if (filterExt != null)
            {
                if (filterExt.Offset > 0)
                    query = query.Skip(filterExt.Offset);
                if (filterExt.Count != int.MaxValue)
                    query = query.Take(filterExt.Count);
            }

            result.Data = query;

            return result;
        }

        private static void BuildPropertyFilter(ParameterExpression li, FilterPropertyViewModel property, ref Expression where)
        {
            switch (property.CompareType)
            {
                case CompareType.Equals:
                    And(Equal(li, property), ref where);
                    break;
                case CompareType.NotEquals:
                    And(NotEqual(li, property), ref where);
                    break;
                case CompareType.Contains:
                    And(Contains(li, property), ref where);
                    break;
                case CompareType.ContainsCollection:
                    And(ContainsCollection(li, property), ref where);
                    break;
                case CompareType.NotContains:
                    And(NotContains(li, property), ref where);
                    break;
                case CompareType.EqualsAMore:
                    And(EqualsAMore(li, property), ref where);
                    break;
                case CompareType.EqualsALess:
                    And(EqualsALess(li, property), ref where);
                    break;
                case CompareType.More:
                    And(More(li, property), ref where);
                    break;
                case CompareType.Less:
                    And(Less(li, property), ref where);
                    break;
                case CompareType.ContainsCase:
                    And(ContainsCase(li, property), ref where);
                    break;
                case CompareType.ContainsIgnoreCase:
                    And(ContainsIgnoreCase(li, property), ref where);
                    break;
                default:
                    break;
            }
        }

        private static void BuildPropertyOrder(ParameterExpression li, FilterPropertyOrderViewModel property, ref IQueryable<T> order, bool first)
        {
            string command = first ?
                property.ASC ? "OrderBy" : "OrderByDescending" :
                property.ASC ? "ThenBy" : "ThenByDescending";

            var p = GetPropertyExp(li, property.PropertyPath);
            var orderByExpression = Expression.Lambda(p, li);

            var result = Expression.Call(typeof(Queryable), command, new Type[] { typeof(T), p.Type },
                                          order.Expression, Expression.Quote(orderByExpression));

            order = order.Provider.CreateQuery<T>(result);
        }

        #region FilterExpressions

        private static Expression trueExpression = Expression.Constant(true, typeof(bool));

        private static MethodInfo stringContainsMethodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        private static MethodInfo arrayContainsMethodInfo = typeof(Enumerable).GetMethods().First(x => x.Name.Equals("Contains") && x.GetParameters().Count() == 2);

        private static MethodInfo stringContainsCaseMethodInfo = typeof(FilterExtensions).GetMethod("ContainsCase", BindingFlags.Public | BindingFlags.Static);

        private static MethodInfo stringContainsIgnoreCaseMethodInfo = typeof(FilterExtensions).GetMethod("ContainsIgnoreCase", BindingFlags.Public | BindingFlags.Static);

        private static Expression ContainsCollection(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, (nameProperty, nameSearch) =>
            {
                var startsWithMethod = arrayContainsMethodInfo.MakeGenericMethod(GetPropertyType(nameProperty));
                var startWithCall = Expression.Call(nameProperty, startsWithMethod, new Expression[] { nameSearch });
                return Expression.Equal(startWithCall, trueExpression);
            });

        private static Expression ContainsCase(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, (nameProperty, nameSearch) =>
            {
                var startWithCall = Expression.Call(null, stringContainsCaseMethodInfo, new Expression[] { nameProperty, nameSearch });
                return Expression.Equal(startWithCall, trueExpression);
            });

        private static Expression ContainsIgnoreCase(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, (nameProperty, nameSearch) =>
            {
                var startWithCall = Expression.Call(null, stringContainsIgnoreCaseMethodInfo, new Expression[] { nameProperty, nameSearch });
                return Expression.Equal(startWithCall, trueExpression);
            });

        private static Expression Contains(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, (nameProperty, nameSearch) =>
            {
                var startWithCall = Expression.Call(nameProperty, stringContainsMethodInfo, new Expression[] { nameSearch });
                return Expression.Equal(startWithCall, trueExpression);
            });

        private static Expression NotContains(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, (nameProperty, nameSearch) =>
            {
                var startWithCall = Expression.Call(nameProperty, stringContainsMethodInfo, new Expression[] { nameSearch });
                return Expression.NotEqual(startWithCall, trueExpression);
            });

        private static Expression Equal(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, Expression.Equal);

        private static Expression NotEqual(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, Expression.NotEqual);

        private static Expression EqualsAMore(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, Expression.GreaterThanOrEqual);

        private static Expression EqualsALess(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, Expression.LessThanOrEqual);

        private static Expression More(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, Expression.GreaterThan);

        private static Expression Less(Expression listOfNames, FilterPropertyViewModel property)
            => BuildBinaryExpressions(listOfNames, property, Expression.LessThan);

        private static BinaryExpression BuildBinaryExpressions(Expression listOfNames, FilterPropertyViewModel property, Func<MemberExpression, Expression, BinaryExpression> build)
        {
            var v = BuildConvertibleExpressions(listOfNames, property);

            return build(v.member, v.value);
        }

        private static (MemberExpression member, ConstantExpression value) BuildConvertibleExpressions(Expression listOfNames, FilterPropertyViewModel property)
        {
            var userIdParam = GetPropertyExp(listOfNames, property.PropertyPath);

            var type = GetPropertyType(userIdParam);

            var userIdValue = Expression.Constant(NormalizePropertyValue(type, property), type);

            return (userIdParam, userIdValue);
        }

        private static void Or(Expression equals, ref Expression op)
        {
            if (op == null)
                op = equals;
            else
                op = Expression.Or(op, equals);
        }

        private static void And(Expression equals, ref Expression op)
        {
            if (op == null)
                op = equals;
            else
                op = Expression.And(op, equals);
        }

        #endregion


        public static MemberExpression GetPropertyExp(Expression t, string path)
        {
            var props = path.Split('.');

            var exp = Expression.Property(t, props[0]);

            if (props.Length > 1)
            {
                for (int i = 1; i < props.Length; i++)
                {
                    exp = Expression.Property(exp, props[i]);
                }
            }

            if (IsNullable(exp.Type))
                exp = Expression.Property(exp, "Value");

            return exp;
        }

        private static object NormalizePropertyValue(Type type, FilterPropertyViewModel property)
        {
            if (type == typeof(string))
                return property.Value;

            if (type.BaseType?.IsAssignableTo(typeof(Enum)) == true)
                return Enum.Parse(type, property.Value);

            if (type.IsAssignableTo(typeof(Guid)))
                return Guid.Parse(property.Value);

            return Convert.ChangeType(property.Value, type);
        }

        private static Type GetPropertyType(MemberExpression p)
        {
            if (p.Type == typeof(string)) // prevent ienumerable<char>
                return p.Type;

            if (p.Type.IsAssignableTo(typeof(IEnumerable)))
            {
                var gtypes = p.Type.GetGenericArguments();

                if (gtypes.Any())
                    return gtypes[0];
            }

            return p.Type;
        }

        private static bool IsNullable(Type t)
        {
            return t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }

}
