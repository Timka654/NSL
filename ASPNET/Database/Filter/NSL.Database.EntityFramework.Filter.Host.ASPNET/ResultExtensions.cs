using Microsoft.AspNetCore.Mvc;
using NSL.Database.EntityFramework.Filter.Models;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Filter.Host.ASPNET
{
    public static class ResultExtensions
    {
        public static async Task<IActionResult> FilteredDataResponseAsync<TData>(this ControllerBase controller, DBSetFilteredResult<TData> result)
            where TData : class
            => controller.FilteredDataResult(await result.GetDataResultAsync());

        public static IActionResult FilteredDataResult<TData>(this ControllerBase controller, FilterResultModel<TData> result)
            => controller.Ok(new { data = result });
    }
}
