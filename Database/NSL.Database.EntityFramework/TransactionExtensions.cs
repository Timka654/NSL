using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework
{
    public static class TransactionExtensions
    {
        public delegate Task<bool> InvokeDelegate<TContext>(TContext db, int n);
        public delegate Task<bool> InvokeDelegate2<TContext>(TContext db, int n, InvokeActionContext invokeContext);
        public delegate Task<bool> InvokeDelegate3<TContext>(TContext db, int n, IServiceProvider serviceProvider);

        public static async Task<bool> InvokeDbTransactionAsync<TContext>(this IServiceProvider services, InvokeDelegate<TContext> action, int maxCount = 100)
            where TContext : DbContext
        {
            bool result = false;

            await using var scope = services.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            result = await context.InvokeInTransactionAsync(action, maxCount);

            return result;
        }

        public static async Task<bool> InvokeInTransactionAsync<TContext>(this TContext context, InvokeDelegate<TContext> action, int maxCount = 100)
            where TContext : DbContext
        {
            int i = 0;

            bool result = false;

            DbUpdateConcurrencyException? latestEx = null;

            while (i < maxCount) // throw count limit for concurrent - m/b can contains logic error 
            {
                try
                {
                    await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
                    {
                        using var t = await context.Database.BeginTransactionAsync();

                        if (await action(context, i++))
                        {
                            await context.SaveChangesAsync();

                            await t.CommitAsync();

                            result = true;
                        }

                        i = maxCount;
                    });
                }
                catch (DbUpdateConcurrencyException dbu)
                {
                    latestEx = dbu;

                    await Task.Delay(100);

                    if (dbu.Entries != null)
                        foreach (var entity in dbu.Entries)
                        {
                            await entity.ReloadAsync();
                        }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            if (result)
                return result;

            if (latestEx != null)
                throw latestEx;

            return false;
        }

        public static async Task<bool> InvokeDbTransactionAsync<TContext>(this IServiceProvider services, InvokeDelegate2<TContext> action, int maxCount = 100)
            where TContext : DbContext
        {
            bool result = false;


            await using var scope = services.CreateAsyncScope();

            InvokeActionContext invokeContext = new InvokeActionContext();
            result = await scope.ServiceProvider.GetRequiredService<TContext>().InvokeInTransactionAsync((db, i) =>
            {
                invokeContext.PostAction = () => Task.CompletedTask;

                return action(db, i, invokeContext);
            }, maxCount);

            if (result)
                await invokeContext.PostAction();

            return result;
        }

        public static async Task<bool> InvokeDbTransactionAsync<TContext>(this IServiceProvider services, InvokeDelegate3<TContext> action, int maxCount = 100)
            where TContext : DbContext
        {
            bool result = false;

            await using var scope = services.CreateAsyncScope();

            result = await scope.ServiceProvider.GetRequiredService<TContext>().InvokeInTransactionAsync((db, i) =>
                {
                    return action(db, i, scope.ServiceProvider);
                }, maxCount);

            return result;
        }
    }

    public class InvokeActionContext
    {
        public Func<Task> PostAction { get; set; }
    }
}
