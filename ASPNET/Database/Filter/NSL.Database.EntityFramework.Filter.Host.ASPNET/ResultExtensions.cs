using Microsoft.AspNetCore.Mvc;
using NSL.Database.EntityFramework.Filter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public static class ResultExtensions
    {
        public static async Task<IActionResult> FilteredDataResponseAsync<TData>(this ControllerBase controller, DBSetFilteredResult<TData> result)
            where TData : class
            => FilteredDataResult(controller, await result.GetDataResultAsync());

        public static IActionResult FilteredDataResult<TData>(this ControllerBase controller, FilterResultModel<TData> result)
            => controller.Ok(new { data = result });
    }
}
