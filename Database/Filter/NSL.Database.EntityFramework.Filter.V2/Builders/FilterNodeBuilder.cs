using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NSL.Database.EntityFramework.Filter.V2.Builders
{
    public class FilterNodeBuilder<TEntity> where TEntity : class
    {
        private readonly FilterNode node;

        public FilterNodeBuilder(FilterNode node)
        {
            this.node = node;
        }

        private FilterNodeBuilder<TEntity> AddFilter(string property, FilterOperator op, string value, bool caseSensitive = false, bool not = false)
        {
            node.Filters ??= new List<EntityFilterBlockModel>();
            node.Filters.Add(new EntityFilterBlockModel
            {
                Property = property,
                Type = op,
                Value = value,
                CaseSensitive = caseSensitive,
                Not = not
            });
            return this;
        }

        public FilterNodeBuilder<TEntity> Where(string property, FilterOperator op, string value, bool caseSensitive = false, bool not = false)
            => AddFilter(property, op, value, caseSensitive, not);

        public FilterNodeBuilder<TEntity> Where(Expression<Func<TEntity, object>> propertyExpression, FilterOperator op, string value, bool caseSensitive = false, bool not = false)
            => AddFilter(FilterUtils.GetPath<TEntity>(propertyExpression), op, value, caseSensitive, not);

        private FilterNodeBuilder<TEntity> AddNode(FilterLogic logic, Action<FilterNodeBuilder<TEntity>> configure)
        {
            node.Nodes ??= new List<FilterNode>();
            var newNode = new FilterNode { Logic = logic };
            var builder = new FilterNodeBuilder<TEntity>(newNode);
            configure(builder);
            node.Nodes.Add(newNode);
            return this;
        }

        public FilterNodeBuilder<TEntity> And(Action<FilterNodeBuilder<TEntity>> configure)
        {
            node.Logic = FilterLogic.And;
            return AddNode(FilterLogic.And, configure);
        }

        public FilterNodeBuilder<TEntity> Or(Action<FilterNodeBuilder<TEntity>> configure)
        {
            node.Logic = FilterLogic.Or;
            return AddNode(FilterLogic.Or, configure);
        }

        public FilterNodeBuilder<TEntity> Any<TCollection>(string property, Action<FilterNodeBuilder<TCollection>> configure) where TCollection : class
        {
            node.Filters ??= new List<EntityFilterBlockModel>();
            var nestedFilter = new FilterNode();
            var builder = new FilterNodeBuilder<TCollection>(nestedFilter);
            configure(builder);

            node.Filters.Add(new EntityFilterBlockModel
            {
                Property = property,
                Type = FilterOperator.Any,
                NestedFilter = nestedFilter
            });
            return this;
        }

        public FilterNodeBuilder<TEntity> Any<TCollection>(Expression<Func<TEntity, object>> propertyExpression, Action<FilterNodeBuilder<TCollection>> configure) where TCollection : class
            => Any(FilterUtils.GetPath(propertyExpression), configure);
    }
}