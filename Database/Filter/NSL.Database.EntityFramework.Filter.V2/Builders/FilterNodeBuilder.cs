using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Models;
using System;
using System.Collections.Generic;
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

        public FilterNodeBuilder<TEntity> AddFilter(EntityFilterBlockModel block)
        {
            node.Filters ??= new List<EntityFilterBlockModel>();
            node.Filters.Add(block);

            return this;
        }
        private FilterNodeBuilder<TEntity> AddFilter(string property, FilterOperator op, object value, bool caseSensitive = false, bool not = false)
            => AddFilter(new EntityFilterBlockModel
            {
                Property = property,
                Type = op,
                Value = value.ToString(),
                CaseSensitive = caseSensitive,
                Not = not
            });

        private FilterNodeBuilder<TEntity> AddFilter(string property, FilterOperator op, FilterNode value, bool not = false)
            => AddFilter(new EntityFilterBlockModel
            {
                Property = property,
                Type = op,
                Not = not,
                NestedFilter = value
            });

        public FilterNodeBuilder<TEntity> Where(Expression<Func<TEntity, object>> propertyExpression, FilterOperator op, FilterNode value, bool not = false)
            => AddFilter(FilterUtils.GetPath(propertyExpression), op, value, not);

        public FilterNodeBuilder<TEntity> Where<TNested>(Expression<Func<TEntity, IEnumerable<TNested>>> propertyExpression, FilterOperator op, Action<FilterNodeBuilder<TNested>> configure, bool not = false)
            where TNested : class
            => Where<TNested>(FilterUtils.GetPath(propertyExpression), op, configure, not);

        public FilterNodeBuilder<TEntity> Where<TNested>(Expression<Func<TEntity, object>> propertyExpression, FilterOperator op, Action<FilterNodeBuilder<TNested>> configure, bool not = false)
            where TNested : class
            => Where<TNested>(FilterUtils.GetPath(propertyExpression), op, configure, not);

        public FilterNodeBuilder<TEntity> Where<TNested>(string property, FilterOperator op, Action<FilterNodeBuilder<TNested>> configure, bool not = false)
            where TNested : class
        {
            var node = new FilterNode();
            var builder = new FilterNodeBuilder<TNested>(node);
            configure(builder);
            return AddFilter(property, op, node, not);
        }

        public FilterNodeBuilder<TEntity> Where<TNested>(Expression<Func<TEntity, TNested>> propertyExpression, FilterOperator op, Action<FilterNodeBuilder<TNested>> configure, bool not = false)
            where TNested : class
        {
            var node = new FilterNode();
            var builder = new FilterNodeBuilder<TNested>(node);
            configure(builder);
            return AddFilter(FilterUtils.GetPath(propertyExpression), op, node, not);
        }

        public FilterNodeBuilder<TEntity> Where(string property, FilterOperator op, object value, bool caseSensitive = false, bool not = false)
            => AddFilter(property, op, value, caseSensitive, not);

        public FilterNodeBuilder<TEntity> Where(Expression<Func<TEntity, object>> propertyExpression, FilterOperator op, object value, bool caseSensitive = false, bool not = false)
            => AddFilter(FilterUtils.GetPath(propertyExpression), op, value, caseSensitive, not);

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