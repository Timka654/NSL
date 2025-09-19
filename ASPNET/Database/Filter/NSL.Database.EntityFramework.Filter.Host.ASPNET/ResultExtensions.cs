using Microsoft.AspNetCore.Mvc;
using NSL.ASPNET.Mvc;
using NSL.Database.EntityFramework.Filter.Models;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Filter.Host.ASPNET
{
    public static class ResultExtensions
    {
        public static DataResponse<EntityFilterResultModel<TData>> FilteredDataResult<TData>(this EntityFilterResultModel<TData> result)
            => DataResponse.Ok(result);

        public static DataResponse<EntityFilterResultModel<TData>> FilteredDataResult<TData>(this EntityFilterResultModel<object> result)
            => DataResponse.Ok<EntityFilterResultModel<TData>>(result);

        public static async Task<DataResponse<EntityFilterResultModel<TData>>> FilteredDataResultAsync<TData>(this Task<EntityFilterResultModel<TData>> result)
            => FilteredDataResult(await result);

        public static async Task<DataResponse<EntityFilterResultModel<TData>>> FilteredDataResultAsync<TData>(this Task<EntityFilterResultModel<object>> result)
            => FilteredDataResult<TData>(await result);
    }
}
