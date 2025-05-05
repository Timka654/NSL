using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace NSL.Database.EntityFramework.ASPNET
{
    public abstract class BaseDBTransactionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int maxRetryCount;
        public Func<ActionExecutedContext, bool> UpdateCondition = (context) => context.Result is IStatusCodeActionResult s && s.StatusCode == 200;

        public BaseDBTransactionAttribute(int maxRetryCount = 25)
        {
            this.maxRetryCount = maxRetryCount;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var dbc = GetContext(context);

            await dbc.InvokeTransactionAsync(async c =>
            {
                var exc = await next();

                var result = UpdateCondition(exc);

                return result;
            }, maxRetryCount);
        }

        protected abstract DbContext GetContext(ActionExecutingContext context);
    }
}
