using NSL.Database.EntityFramework.Filter.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NSL.Database.EntityFramework.Filter.V2.Host
{
    public static class EntityFilterQueryExtensions
    {
        /// <summary>
        /// Applies a complex filter, sorting, and pagination model to an IQueryable source.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="query">The source IQueryable.</param>
        /// <param name="model">The query model containing filter, sort, and pagination info.</param>
        /// <returns>A new IQueryable with all query parameters applied.</returns>
        public static IQueryable<T> WithQueryModel<T, TModel>(this IQueryable<T> query, TModel model)
            where T : class
            where TModel : BaseFilteringQueryModel
        {
            if (model == null)
                return query;

            if (model is IFilterFilteringQueryModel filterModel)
                query = query.WithFilter(filterModel.Filter);

            if (model is ISortedFilteringQueryModel sortModel)
                query = query.WithSorting(sortModel.Sortings);

            if (model is IPaginationFilteringQueryModel pageModel)
                query = query.WithPagination(pageModel.Skip, pageModel.Take);

            if (model is IIncludeFilteringQueryModel includeModel)
                query = query.WithIncludes(includeModel.Includes);

            return query;
        }

        public static IQueryable<T> WithFilter<T>(
            this IQueryable<T> query,
            FilterNode rootNode,
            Action<EntityFilterBuilder<T>> options = null)
            where T : class
        {
            if (rootNode == null)
                return query;

            var builder = new EntityFilterBuilder<T>();
            options?.Invoke(builder);

            // p0 - корневой параметр для выражения
            var parameter = Expression.Parameter(typeof(T), "p0");

            var whereExpression = builder.BuildExpressionFromNode(parameter, rootNode);

            if (whereExpression == null)
                return query;

            return query.Where(Expression.Lambda<Func<T, bool>>(whereExpression, parameter));
        }

        /// <summary>
        /// Applies multi-level sorting to an IQueryable source.
        /// </summary>
        public static IQueryable<T> WithSorting<T>(this IQueryable<T> query, IEnumerable<SortModel> sortings) where T : class
        {
            if (sortings == null || !sortings.Any())
                return query;

            IOrderedQueryable<T> orderedQuery = null;

            foreach (var sort in sortings)
            {
                if (string.IsNullOrWhiteSpace(sort.Property))
                    continue;

                var parameter = Expression.Parameter(typeof(T), "p");
                Expression propertyAccess = parameter;
                foreach (var property in sort.Property.Split('.'))
                {
                    propertyAccess = Expression.Property(propertyAccess, property);
                }
                var lambda = Expression.Lambda(propertyAccess, parameter);

                if (orderedQuery == null) // First sort operation
                {
                    var methodName = sort.Descending ? "OrderByDescending" : "OrderBy";
                    var resultExpression = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new Type[] { typeof(T), propertyAccess.Type },
                        query.Expression,
                        Expression.Quote(lambda));
                    orderedQuery = (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(resultExpression);
                }
                else // Subsequent sort operations
                {
                    var methodName = sort.Descending ? "ThenByDescending" : "ThenBy";
                    var resultExpression = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new Type[] { typeof(T), propertyAccess.Type },
                        orderedQuery.Expression,
                        Expression.Quote(lambda));
                    orderedQuery = (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(resultExpression);
                }
            }

            return orderedQuery ?? query;
        }


        /// <summary>
        /// Applies pagination to an IQueryable source.
        /// </summary>
        public static IQueryable<T> WithPagination<T>(this IQueryable<T> query, int? skip, int? take) where T : class
        {
            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query;
        }

        /// <summary>
        /// Dynamically includes navigation properties for eager loading.
        /// </summary>
        public static IQueryable<T> WithIncludes<T>(this IQueryable<T> query, IEnumerable<string> includePaths) where T : class
        {
            if (includePaths == null)
                return query;

            foreach (var path in includePaths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    query = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(query, path);
                }
            }
            return query;
        }
    }
}