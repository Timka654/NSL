using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NSL.Database.EntityFramework.Filter.Host.ASPNET
{
    public record EntityFilterQueryModel<T>(IQueryable<T> Data, IQueryable<T> Pagging);

    public static class DBQueryUtils
    {
        public static EntityFilterQueryModel<T> WithEntityFilter<T, TModel>(this IQueryable<T> query, TModel model) where T : class where TModel : BaseFilteringQueryModel
        {
            if (model == null)
            {
                return new EntityFilterQueryModel<T>(query, query);
            }

            if (model is IFilterFilteringQueryModel filterFilteringQueryModel)
            {
                query = query.WithFilter(filterFilteringQueryModel.Filter);
            }

            var pagging = query;

            if (model is ISortedFilteringQueryModel sortedFilteringQueryModel)
            {
                query = query.WithSorting(sortedFilteringQueryModel.Sortings);
            }

            if (model is IPaginationFilteringQueryModel paginationFilteringQueryModel)
            {
                query = query.WithPagination(paginationFilteringQueryModel.Skip, paginationFilteringQueryModel.Take);
            }

            if (model is IIncludeFilteringQueryModel includeFilteringQueryModel)
            {
                query = query.WithIncludes(includeFilteringQueryModel.Includes);
            }

            return new EntityFilterQueryModel<T>(query, pagging);
        }

        public static async Task<FilterResultModel<TResult>> GetResponse<T, TResult>(this EntityFilterQueryModel<T> query, Func<IQueryable<T>, IQueryable<TResult>> builder, CancellationToken cancellationToken = default(CancellationToken))
        {

            FilterResultModel<TResult> entityFilterResultModel = new FilterResultModel<TResult>();

            entityFilterResultModel.Data = await builder(query.Data)
                .ToArrayAsync(cancellationToken);

            entityFilterResultModel.Count = await query.Pagging
                .LongCountAsync(cancellationToken);

            return entityFilterResultModel;
        }
    }
}
