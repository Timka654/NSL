using NSL.Database.EntityFramework.Filter.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NSL.Database.EntityFramework.Filter.V2.Builders
{
    public abstract class FilteredQueryBuilder
    {
        public static FilteredQueryBuilder<TModel, TEntity> Create<TModel, TEntity>()
            where TModel : BaseFilteringQueryModel, new()
            where TEntity : class
        {
            return new FilteredQueryBuilder<TModel, TEntity>();
        }
        public static FilteredQueryBuilder<TModel,object> Create<TModel>()
            where TModel : BaseFilteringQueryModel, new()
        {
            return new FilteredQueryBuilder<TModel, object>();
        }
    }

    public class FilteredQueryBuilder<TModel, TEntity>
        where TModel : BaseFilteringQueryModel, new()
        where TEntity : class
    {
        private readonly TModel model = new();

        public FilteredQueryBuilder()
        {
        }

        public FilteredQueryBuilder<TModel, TEntity> WithFilter(Action<FilterNodeBuilder<TEntity>> configure)
        {
            if (model is not IFilterFilteringQueryModel f) throw new InvalidOperationException("Model does not support filtering");

            f.Filter ??= new FilterNode();

            var builder = new FilterNodeBuilder<TEntity>(f.Filter);
            configure(builder);
            return this;
        }

        public FilteredQueryBuilder<TModel, TEntity> OrderBy(string property)
        {
            if (model is not ISortedFilteringQueryModel s) throw new InvalidOperationException("Model does not support sorting");

            s.Sortings ??= new List<SortModel>();
            s.Sortings.Add(new SortModel { Property = property, Descending = false });

            return this;
        }

        public FilteredQueryBuilder<TModel, TEntity> OrderBy(Expression<Func<TEntity, object>> propertyExpression)
            => OrderBy(FilterUtils.GetPath<TEntity>(propertyExpression));

        public FilteredQueryBuilder<TModel, TEntity> OrderByDescending(string property)
        {
            if (model is not ISortedFilteringQueryModel s) throw new InvalidOperationException("Model does not support sorting");

            s.Sortings ??= new List<SortModel>();
            s.Sortings.Add(new SortModel { Property = property, Descending = true });
            return this;
        }

        public FilteredQueryBuilder<TModel, TEntity> OrderByDescending(Expression<Func<TEntity, object>> propertyExpression)
            => OrderByDescending(FilterUtils.GetPath<TEntity>(propertyExpression));

        public FilteredQueryBuilder<TModel, TEntity> Skip(int count)
        {
            if (model is not IPaginationFilteringQueryModel p) throw new InvalidOperationException("Model does not support pagination");

            p.Skip = count;
            return this;
        }

        public FilteredQueryBuilder<TModel, TEntity> Take(int count)
        {
            if (model is not IPaginationFilteringQueryModel p) throw new InvalidOperationException("Model does not support pagination");

            p.Take = count;
            return this;
        }

        public FilteredQueryBuilder<TModel, TEntity> Include(params string[] paths)
        {
            if (model is not IIncludeFilteringQueryModel i) throw new InvalidOperationException("Model does not support includes");

            i.Includes ??= new List<string>();
            i.Includes.AddRange(paths);
            return this;
        }

        public FilteredQueryBuilder<TModel, TEntity> Include(Expression<Func<TEntity, object>> propertyExpression)
            => Include(FilterUtils.GetPath<TEntity>(propertyExpression));

        public FilteredQueryBuilder<TModel, TEntity> Select(params string[] paths)
        {
            if (model is not ISelectFilteringQueryModel s) throw new InvalidOperationException("Model does not support selection");

            s.Properties ??= new List<string>();
            s.Properties.AddRange(paths);

            return this;
        }

        public FilteredQueryBuilder<TModel, TEntity> Select(params Expression<Func<TEntity, object>>[] propertyExpression)
            => Select(propertyExpression.Select(x=>FilterUtils.GetPath<TEntity>(x)).ToArray());


        public TModel Build()
        {
            return model;
        }
    }
}