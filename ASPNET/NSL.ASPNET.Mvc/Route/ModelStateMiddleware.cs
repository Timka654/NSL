using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace NSL.ASPNET.Mvc.Route
{
    public class ModelStateFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = ControllerResults.ModelStateResponse(context.ModelState);
            }
        }
    }

    public static class ModelStateRegisterExtensions
    {
        public static TBuilder AddNSLModelStateFilter<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            builder.Services.AddMvc(o => { o.Filters.Add(new ModelStateFilterAttribute()); });

            return builder;
        }
    }
}
