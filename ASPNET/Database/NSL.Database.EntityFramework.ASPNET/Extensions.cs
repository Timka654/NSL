using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using NSL.ASPNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;

namespace NSL.Database.EntityFramework.ASPNET
{
    public static class Extensions
    {
        public static async Task AcceptDbMigrations<TContext>(this IHost host, CancellationToken cancellationToken = default)
            where TContext : DbContext
        {
            await host.Services.InvokeInScopeAsync(async services =>
            {
                await services.AcceptDbMigrations<TContext>(cancellationToken);
            });
        }

        public static async Task AcceptDbMigrations<TContext>(this IServiceProvider services, CancellationToken cancellationToken = default)
            where TContext : DbContext
        {
            await services.GetRequiredService<TContext>().Database.MigrateAsync(cancellationToken);
        }

        public static async Task ThrowIfExistsDbMigrations<TContext>(this IHost host, Func<Exception> throwBuilder, CancellationToken cancellationToken = default)
            where TContext : DbContext
        {
            await host.Services.InvokeInScopeAsync(async services =>
            {
                await services.ThrowIfExistsDbMigrations<TContext>(throwBuilder, cancellationToken);
            });
        }

        public static async Task ThrowIfExistsDbMigrations<TContext>(this IServiceProvider services, Func<Exception> throwBuilder, CancellationToken cancellationToken = default)
            where TContext : DbContext
        {
            var p = await services.GetRequiredService<TContext>().Database.GetPendingMigrationsAsync(cancellationToken);

            if (p.Any())
                throw throwBuilder();
        }

        public static async Task<bool> HaveAcceptedDbMigrations<TContext>(this IHost host, CancellationToken cancellationToken = default)
            where TContext : DbContext
        {
            bool result = false;

            await host.Services.InvokeInScopeAsync(async services =>
            {
                result = await services.HaveAcceptedDbMigrations<TContext>(cancellationToken);
            });

            return result;
        }

        public static async Task<bool> HaveAcceptedDbMigrations<TContext>(this IServiceProvider services, CancellationToken cancellationToken = default)
            where TContext : DbContext
        {
            var p = await services.GetRequiredService<TContext>().Database.GetAppliedMigrationsAsync(cancellationToken);

            return p.Any();
        }

        public static async Task<bool> HaveAcceptedDbMigration<TContext>(this IHost host, string name, CancellationToken cancellationToken = default)
            where TContext : DbContext
        {
            bool result = false;

            await host.Services.InvokeInScopeAsync(async services =>
            {
                result = await services.HaveAcceptedDbMigration<TContext>(name, cancellationToken);
            });

            return result;
        }

        public static async Task<bool> HaveAcceptedDbMigration<TContext>(this IServiceProvider services, string name, CancellationToken cancellationToken = default)
            where TContext : DbContext
        {
            var p = await services.GetRequiredService<TContext>().Database.GetAppliedMigrationsAsync(cancellationToken);

            return p.Any(x => string.Equals(x, name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
