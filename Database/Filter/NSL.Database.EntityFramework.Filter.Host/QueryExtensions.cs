using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Finalize postgres query with insert commented parameters to query string
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static string ToPgQueryString(this IQueryable q)
        {
            var query = q.ToQueryString();
            var querySplit = query.Split(Environment.NewLine);

            var parameters = new List<(string, string)>();

            int l = 0;
            bool started = false;
            for (; l < querySplit.Length; l++)
            {
                var line = querySplit[l];

                if (!line.StartsWith("-- @"))
                {
                    if (started)
                        break;
                    continue;
                }

                started = true;

                var m = Regex.Match(line, "^-- (\\s*\\S*)=(\\s*\\S*)");
                parameters.Add((m.Groups[1].Value, m.Groups[2].Value));
            }

            query = string.Join(Environment.NewLine, querySplit[l..]);

            foreach (var parameter in parameters)
            {
                query = query.Replace(parameter.Item1, parameter.Item2);
            }

            return query;
        }
    }
}
