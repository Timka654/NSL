using NSL.Database.EntityFramework.Filter.Host;
using NSL.Database.EntityFramework.Filter.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public static class QueryExtensions
    {
        public static DBSetFilteredResult<T> Filter<T>(this IQueryable<T> set, EntityFilterQueryModel query)
            where T : class
        {
            return DBSetFilter<T>.Filter(set, query);
        }

        public static DBSetFilteredResult<T> Filter<T>(this IQueryable<T> set, Func<IQueryable<T>, IQueryable<T>> queryBuilder, EntityFilterQueryModel query)
            where T : class
        {
            return DBSetFilter<T>.Filter(queryBuilder(set), query);
        }

        public static async Task<FilterResultModel<T>> ToDataResultAsync<T>(this DBSetFilteredResult<T> query)
            where T : class
        {
            return await query.GetDataResultAsync(query.Data);
        }

        public static async Task<FilterResultModel<TResult>> ToDataResultAsync<T, TResult>(this DBSetFilteredResult<T> query, Func<IQueryable<T>, IQueryable<TResult>> builder)
            where T : class
        {
            return await query.GetDataResultAsync(builder(query.Data));
        }
    }
}
