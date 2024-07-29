using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using NSL.ASPNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

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

        public static async Task AcceptDbMigrations<TContext>(this IHost host)
            where TContext : DbContext
        {
            await host.Services.InvokeInScopeAsync(async services =>
            {
                await services.AcceptDbMigrations<TContext>();
            });
        }

        public static async Task AcceptDbMigrations<TContext>(this IServiceProvider services)
            where TContext : DbContext
        {
            await services.GetRequiredService<TContext>().Database.MigrateAsync();
        }

        public static async Task ThrowIfExistsDbMigrations<TContext>(this IHost host, Func<Exception> throwBuilder)
            where TContext : DbContext
        {
            await host.Services.InvokeInScopeAsync(async services =>
            {
                await services.ThrowIfExistsDbMigrations<TContext>(throwBuilder);
            });
        }

        public static async Task ThrowIfExistsDbMigrations<TContext>(this IServiceProvider services, Func<Exception> throwBuilder)
            where TContext : DbContext
        {
            var p = await services.GetRequiredService<TContext>().Database.GetPendingMigrationsAsync();

            if (p.Any())
                throw throwBuilder();
        }

        public static async Task<bool> HaveAcceptedDbMigrations<TContext>(this IHost host)
            where TContext : DbContext
        {
            bool result = false;
            
            await host.Services.InvokeInScopeAsync(async services =>
            {
                result = await services.HaveAcceptedDbMigrations<TContext>();
            });

            return result;
        }

        public static async Task<bool> HaveAcceptedDbMigrations<TContext>(this IServiceProvider services)
            where TContext : DbContext
        {
            var p = await services.GetRequiredService<TContext>().Database.GetAppliedMigrationsAsync();

            return p.Any();
        }

        public static async Task<bool> HaveAcceptedDbMigration<TContext>(this IHost host, string name)
            where TContext : DbContext
        {
            bool result = false;
            
            await host.Services.InvokeInScopeAsync(async services =>
            {
                result = await services.HaveAcceptedDbMigration<TContext>(name);
            });

            return result;
        }

        public static async Task<bool> HaveAcceptedDbMigration<TContext>(this IServiceProvider services, string name)
            where TContext : DbContext
        {
            var p = await services.GetRequiredService<TContext>().Database.GetAppliedMigrationsAsync();

            return p.Any(x=> string.Equals(x, name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
