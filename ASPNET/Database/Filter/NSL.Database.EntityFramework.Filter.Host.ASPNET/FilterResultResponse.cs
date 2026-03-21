using NSL.Database.EntityFramework.Filter.V2.Models;

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
