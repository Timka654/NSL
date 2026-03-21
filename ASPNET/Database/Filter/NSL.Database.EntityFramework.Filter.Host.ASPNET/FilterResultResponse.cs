using NSL.Database.EntityFramework.Filter.V2.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Filter.Host.ASPNET
{
    public class FilterResultResponse
    {
        public static FilterResultResponse<TData> Ok<TData>(FilterResultModel<TData> data)
        {
            return new FilterResultResponse<TData>(data);
        }

        public static FilterResultResponse<TData> Ok<TData>(object data)
        {
            return new FilterResultResponse<TData>(200, data);
        }

        public static FilterResultResponse<TData> Forbid<TData>()
        {
            return new FilterResultResponse<TData>(403, null);
        }
    }

    public static class TEx
    {
        public static async Task<FilterResultResponse<TModel>> ToResponse<TModel>(this EntityFilterQueryModel<TModel> query, Func<IQueryable<TModel>, IQueryable<object>> selector, CancellationToken cancellationToken)
        {
            var response = query.GetData(selector, cancellationToken);
            return await response.ToResponse<TModel>();
        }

        public static async Task<FilterResultResponse<TModel>> ToResponse<TModel>(this Task<FilterResultModel<object>> response)
        {
            var result = await response;

            return FilterResultResponse.Ok<TModel>(result);
        }
    }

    public class FilterResultResponse<TEntity> : NSL.ASPNET.Mvc.DataResponse<FilterResultModel<TEntity>>
    {
        public FilterResultResponse(FilterResultModel<TEntity> data) : base(data)
        {
        }

        public FilterResultResponse(int statusCode, object? data) : base(statusCode, new { data })
        {
        }
    }
}
