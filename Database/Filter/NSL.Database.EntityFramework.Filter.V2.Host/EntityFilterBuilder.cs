using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Enums.NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NSL.Database.EntityFramework.Filter.V2.Host
{
    public class EntityFilterBuilder<T> where T : class
    {
        private int parameterCounter = 1; // Начинаем с 1, так как p0 - корневой

        private static readonly MethodInfo StringEqualsMethod = typeof(DbFilterFunctions).GetMethod(nameof(DbFilterFunctions.Equals), new[] { typeof(string), typeof(string), typeof(bool) });
        private static readonly MethodInfo StringContainsMethod = typeof(DbFilterFunctions).GetMethod(nameof(DbFilterFunctions.Contains), new[] { typeof(string), typeof(string), typeof(bool) });
        private static readonly MethodInfo StringStartsWithMethod = typeof(DbFilterFunctions).GetMethod(nameof(DbFilterFunctions.StartsWith), new[] { typeof(string), typeof(string), typeof(bool) });
        private static readonly MethodInfo StringEndsWithMethod = typeof(DbFilterFunctions).GetMethod(nameof(DbFilterFunctions.EndsWith), new[] { typeof(string), typeof(string), typeof(bool) });

        private static readonly MethodInfo EnumerableAnySimpleMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Any" && m.GetParameters().Length == 1);

        private static readonly MethodInfo EnumerableAnyWithPredicateMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2);

        private static readonly MethodInfo EnumerableCountMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Count" && m.GetParameters().Length == 1);

        private static readonly MethodInfo EnumerableCountWithPredicateMethod = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Count" && m.GetParameters().Length == 2);

        private readonly Dictionary<FilterOperator, Func<ParameterExpression, EntityFilterBlockModel, Expression>> _filterHandlers;

        public EntityFilterBuilder()
        {
            _filterHandlers = new Dictionary<FilterOperator, Func<ParameterExpression, EntityFilterBlockModel, Expression>>
            {
                { FilterOperator.Equal, BuildEqualExpression },
                { FilterOperator.GreaterThan, BuildBinaryExpression(Expression.GreaterThan) },
                { FilterOperator.LessThan, BuildBinaryExpression(Expression.LessThan) },
                { FilterOperator.GreaterThanOrEqual, BuildBinaryExpression(Expression.GreaterThanOrEqual) },
                { FilterOperator.LessThanOrEqual, BuildBinaryExpression(Expression.LessThanOrEqual) },
                { FilterOperator.Contains, BuildStringExpression(StringContainsMethod) },
                { FilterOperator.StartsWith, BuildStringExpression(StringStartsWithMethod) },
                { FilterOperator.EndsWith, BuildStringExpression(StringEndsWithMethod) }
                // `Any` удален из словаря
            };
        }

        public Expression BuildExpressionFromNode(ParameterExpression parameter, FilterNode node)
        {
            if (node == null)
                return null;

            var expressions = new List<Expression>();

            if (node.Filters?.Any() == true)
            {
                foreach (var filter in node.Filters)
                {
                    var expr = BuildExpression(parameter, filter);
                    if (expr != null)
                        expressions.Add(expr);
                }
            }

            if (node.Nodes?.Any() == true)
            {
                foreach (var childNode in node.Nodes)
                {
                    var childExpr = BuildExpressionFromNode(parameter, childNode);
                    if (childExpr != null)
                        expressions.Add(childExpr);
                }
            }

            if (expressions.Count == 0)
                return null;
            if (expressions.Count == 1)
                return expressions[0];

            Func<Expression, Expression, BinaryExpression> combineFunc = node.Logic == FilterLogic.And ? Expression.AndAlso : Expression.OrElse;
            return expressions.Aggregate(combineFunc);
        }

        public Expression BuildExpression(ParameterExpression parameter, EntityFilterBlockModel item)
        {
            if (string.IsNullOrWhiteSpace(item.Property))
                return null;

            Expression expression;

            // `Any` обрабатывается как особый случай
            if (item.Type == FilterOperator.Any)
            {
                expression = BuildAnyExpression(parameter, item);
            }
            else
            {
                if (!_filterHandlers.TryGetValue(item.Type, out var handler))
                    throw new NotSupportedException($"Filter type '{item.Type}' is not supported.");

                expression = handler(parameter, item);
            }

            return item.Not ? Expression.Not(expression) : expression;
        }

        private Expression BuildEqualExpression(ParameterExpression parameter, EntityFilterBlockModel item)
        {
            var (member, value) = GetMemberAndValue(parameter, item);

            if (member.Type == typeof(string))
            {
                return Expression.Call(null, StringEqualsMethod, member, value, Expression.Constant(item.CaseSensitive));
            }

            // Для Enum, `Equal` работает без приведения типов, так как он определен
            return Expression.Equal(member, value);
        }

        private Func<ParameterExpression, EntityFilterBlockModel, Expression> BuildBinaryExpression(Func<Expression, Expression, BinaryExpression> op)
        {
            return (parameter, item) =>
            {
                var (member, value) = GetMemberAndValue(parameter, item);

                var underlyingType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;

                // Если это Enum, для операторов сравнения (> < и т.д.) необходимо приведение к числовому типу
                if (underlyingType.IsEnum)
                {
                    var enumUnderlyingType = Enum.GetUnderlyingType(underlyingType);
                    var convertedMember = Expression.Convert(member, enumUnderlyingType);
                    var convertedValue = Expression.Convert(value, enumUnderlyingType);
                    return op(convertedMember, convertedValue);
                }

                return op(member, value);
            };
        }

        private Func<ParameterExpression, EntityFilterBlockModel, Expression> BuildStringExpression(MethodInfo method)
        {
            return (parameter, item) =>
            {
                var (member, value) = GetMemberAndValue(parameter, item);

                if (member.Type != typeof(string))
                    throw new InvalidOperationException($"Operator '{item.Type}' can only be applied to string properties.");

                return Expression.Call(null, method, member, value, Expression.Constant(item.CaseSensitive));
            };
        }

        private Expression BuildAnyExpression(ParameterExpression parameter, EntityFilterBlockModel item)
        {
            var collectionProperty = GetPropertyExpression(parameter, item.Property);

            if (!typeof(IEnumerable).IsAssignableFrom(collectionProperty.Type) || collectionProperty.Type == typeof(string))
                throw new InvalidOperationException($"Property '{item.Property}' must be a collection to use the 'Any' operator.");

            var itemType = GetCollectionItemType(collectionProperty.Type);

            if (item.NestedFilter != null)
            {
                var nestedParameter = Expression.Parameter(itemType, $"p{parameterCounter++}");
                var lambdaBody = BuildExpressionFromNode(nestedParameter, item.NestedFilter);

                if (lambdaBody == null)
                {
                    var anyMethod = EnumerableAnySimpleMethod.MakeGenericMethod(itemType);
                    return Expression.Call(null, anyMethod, collectionProperty);
                }

                var anyPredicateMethod = EnumerableAnyWithPredicateMethod.MakeGenericMethod(itemType);
                var lambda = Expression.Lambda(lambdaBody, nestedParameter);
                return Expression.Call(null, anyPredicateMethod, collectionProperty, lambda);
            }
            else
            {
                var anyMethod = EnumerableAnySimpleMethod.MakeGenericMethod(itemType);
                return Expression.Call(null, anyMethod, collectionProperty);
            }
        }

        private (Expression member, Expression value) GetMemberAndValue(ParameterExpression parameter, EntityFilterBlockModel item)
        {
            var memberExp = GetPropertyExpression(parameter, item.Property);

            if (item.Modifier == PropertyModifier.Count)
            {
                memberExp = BuildCountExpression(memberExp, item);
            }

            var underlyingType = Nullable.GetUnderlyingType(memberExp.Type) ?? memberExp.Type;

            object parsedValue = NormalizePropertyValue(underlyingType, item.Value);

            var valueExp = Expression.Constant(parsedValue, memberExp.Type);

            return (memberExp, valueExp);
        }

        private Expression BuildCountExpression(Expression collectionProperty, EntityFilterBlockModel item)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(collectionProperty.Type) || collectionProperty.Type == typeof(string))
            {
                throw new InvalidOperationException($"Modifier '{PropertyModifier.Count}' can only be applied to collection properties. Property is not a collection.");
            }

            var itemType = GetCollectionItemType(collectionProperty.Type);

            if (item.NestedFilter != null)
            {
                var countMethod = EnumerableCountWithPredicateMethod.MakeGenericMethod(itemType);
                var nestedParameter = Expression.Parameter(itemType, $"p{parameterCounter++}");
                var lambdaBody = BuildExpressionFromNode(nestedParameter, item.NestedFilter);

                if (lambdaBody == null)
                {
                    var simpleCountMethod = EnumerableCountMethod.MakeGenericMethod(itemType);
                    return Expression.Call(null, simpleCountMethod, collectionProperty);
                }

                var lambda = Expression.Lambda(lambdaBody, nestedParameter);
                return Expression.Call(null, countMethod, collectionProperty, lambda);
            }
            else
            {
                var simpleCountMethod = EnumerableCountMethod.MakeGenericMethod(itemType);
                return Expression.Call(null, simpleCountMethod, collectionProperty);
            }
        }

        private Expression GetPropertyExpression(Expression expression, string path)
        {
            try
            {
                return path.Split('.').Aggregate(expression, Expression.PropertyOrField);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"Invalid property path: '{path}' on type '{typeof(T).Name}'.", ex);
            }
        }

        private object NormalizePropertyValue(Type targetType, string value)
        {
            if (value == null)
                return null;

            if (targetType.IsEnum)
            {
                if (int.TryParse(value, out var intValue))
                {
                    return Enum.ToObject(targetType, intValue);
                }
                return Enum.Parse(targetType, value, true);
            }

            if (targetType == typeof(Guid))
                return Guid.Parse(value);

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private bool IsNullable(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);

        private Type GetCollectionItemType(Type collectionType)
        {
            return collectionType.GetElementType() ?? collectionType.GetGenericArguments().FirstOrDefault();
        }
    }
}