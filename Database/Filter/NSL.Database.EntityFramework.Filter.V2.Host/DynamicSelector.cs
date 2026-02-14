using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NSL.Database.EntityFramework.Filter.V2.Host
{
    public static class DynamicSelector
    {
        // Кэшируем результаты рефлексии для производительности
        private static readonly MethodInfo AddMethod = typeof(Dictionary<string, object>).GetMethod("Add", new[] { typeof(string), typeof(object) });
        private static readonly MethodInfo SelectMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2);

        /// <summary>
        /// Projects the IQueryable into a sequence of dictionaries, selecting only the specified properties.
        /// Supports nested properties and collections.
        /// </summary>
        public static IQueryable<Dictionary<string, object>> SelectDynamic<T>(this IQueryable<T> query, IEnumerable<string> propertyNames) where T : class
        {
            if (propertyNames == null || !propertyNames.Any())
                return query.Select(e => new Dictionary<string, object>());

            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyTree = BuildPropertyTree(propertyNames);
            var selector = BuildSelectorExpression(parameter, propertyTree);

            var lambda = Expression.Lambda<Func<T, Dictionary<string, object>>>(selector, parameter);

            return query.Select(lambda);
        }

        private class PropertyNode
        {
            public string Name { get; set; }
            public List<PropertyNode> Children { get; } = new List<PropertyNode>();
        }

        private static List<PropertyNode> BuildPropertyTree(IEnumerable<string> propertyNames)
        {
            var root = new PropertyNode();

            foreach (var name in propertyNames.OrderBy(n => n)) // Сортируем для консистентности
            {
                var currentNode = root;
                foreach (var part in name.Split('.'))
                {
                    var childNode = currentNode.Children.FirstOrDefault(c => c.Name == part);
                    if (childNode == null)
                    {
                        childNode = new PropertyNode { Name = part };
                        currentNode.Children.Add(childNode);
                    }
                    currentNode = childNode;
                }
            }
            return root.Children;
        }

        private static Expression BuildSelectorExpression(Expression parentExpression, List<PropertyNode> nodes)
        {
            var newDictExpression = Expression.New(typeof(Dictionary<string, object>));
            var initializers = new List<ElementInit>();

            foreach (var node in nodes)
            {
                var propertyExpression = Expression.PropertyOrField(parentExpression, node.Name);
                Expression valueExpression;

                if (node.Children.Any())
                {
                    if (IsCollectionType(propertyExpression.Type))
                    {
                        var collectionType = GetCollectionElementType(propertyExpression.Type);
                        var collectionParam = Expression.Parameter(collectionType, $"c_{collectionType.Name}");
                        var nestedSelector = BuildSelectorExpression(collectionParam, node.Children);
                        var selectLambda = Expression.Lambda(nestedSelector, collectionParam);

                        // Используем кэшированный MethodInfo
                        var genericSelectMethod = SelectMethod.MakeGenericMethod(collectionType, typeof(Dictionary<string, object>));
                        valueExpression = Expression.Call(null, genericSelectMethod, propertyExpression, selectLambda);
                    }
                    else
                    {
                        valueExpression = BuildSelectorExpression(propertyExpression, node.Children);
                    }
                }
                else
                {
                    valueExpression = propertyExpression;
                }

                var convertedValue = Expression.Convert(valueExpression, typeof(object));
                // Используем кэшированный MethodInfo
                initializers.Add(Expression.ElementInit(AddMethod, Expression.Constant(node.Name), convertedValue));
            }

            return Expression.ListInit(newDictExpression, initializers);
        }

        private static bool IsCollectionType(Type type)
        {
            if (type == typeof(string)) return false;
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static Type GetCollectionElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            var ienumerable = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return ienumerable?.GetGenericArguments()[0];
        }
    }
}