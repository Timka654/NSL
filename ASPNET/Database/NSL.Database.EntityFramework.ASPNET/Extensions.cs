using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using NSL.ASPNET;
using Microsoft.Extensions.DependencyInjection;

namespace NSL.Database.EntityFramework.ASPNET
{
    public static class Extensions
    {
        public static async Task<bool> InvokeDbTransactionAsync<TContext>(this IServiceProvider services, TransactionExtensions.InvokeDelegate<TContext> action)
            where TContext : DbContext
        {
            bool result = false;

            await services.InvokeInScopeAsync(async provider =>
            {
                result = await provider.GetRequiredService<TContext>().InvokeInTransactionAsync(action);
            });

            return result;
        }
    }
}
