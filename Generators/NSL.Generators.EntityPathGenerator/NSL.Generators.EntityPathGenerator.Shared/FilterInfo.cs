using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NSL.Generators.EntityPathGenerator.Shared
{
    public class FilterInfo
    {
        public FilterInfo(Type PropertyType,
            LambdaExpression Expression,
            Func<IReadOnlyDictionary<string, FilterInfo>> Nested,
            bool IsCollection,
            Type ElementType,
            string[] Models,
            bool CanBeNull,
            IReadOnlyDictionary<string, object> Meta)
        {
            this.PropertyType = PropertyType;
            this.Expression = Expression;
            this.Nested = Nested;
            this.IsCollection = IsCollection;
            this.ElementType = ElementType;
            this.Models = Models;
            this.CanBeNull = CanBeNull;
            this.Meta = Meta;
        }

        public Type PropertyType { get; }
        public LambdaExpression Expression { get; }
        public Func<IReadOnlyDictionary<string, FilterInfo>> Nested { get; }
        public bool IsCollection { get; }
        public Type ElementType { get; } // Тип T, если это List<T>
        public string[] Models { get; }    // Для фильтрации по моделям
        public bool CanBeNull { get; }
        public IReadOnlyDictionary<string, object> Meta { get; }
    }
}
