using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework
{
    public static class TransactionExtensions
    {
        public delegate Task<bool> InvokeDelegate<TContext>(TContext db);
        public delegate Task<bool> InvokeDelegate2<TContext>(TContext db, InvokeDbActionContext invokeContext);

        public static async Task<bool> InvokeDbTransactionAsync<TContext>(this IServiceProvider services, InvokeDelegate<TContext> action, int maxCount = 100)
            where TContext : DbContext
        {
            await using var scope = services.CreateAsyncScope();

            return await scope.ServiceProvider.GetRequiredService<TContext>()
                .InvokeTransactionAsync(action, maxCount);
        }

        public static async Task<bool> InvokeDbTransactionAsync<TContext>(this IServiceProvider services, InvokeDelegate2<TContext> action, int maxCount = 100)
            where TContext : DbContext
        {
            await using var scope = services.CreateAsyncScope();

            InvokeDbActionContext invokeContext = new()
            {
            };

            var result = await scope.ServiceProvider.GetRequiredService<TContext>().InvokeTransactionAsync((db) =>
            {
                invokeContext.PostAction = () => Task.CompletedTask;

                ++invokeContext.Iter;

                return action(db, invokeContext);
            }, maxCount);

            if (result && invokeContext.PostAction != null)
                await invokeContext.PostAction();

            return result;
        }

        public static async Task<bool> InvokeTransactionAsync<TContext>(this TContext context, InvokeDelegate<TContext> action, int maxCount = 100)
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

                        if (await action(context))
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

        public static async Task<bool> InvokeTransactionAsync<TContext>(this TContext context, InvokeDelegate2<TContext> action, int maxCount = 100)
            where TContext : DbContext
        {
            InvokeDbActionContext invokeContext = new()
            {
            };

            var result = await context.InvokeTransactionAsync((db) =>
            {
                invokeContext.PostAction = () => Task.CompletedTask;

                ++invokeContext.Iter;

                return action(db, invokeContext);
            }, maxCount);

            if (result && invokeContext.PostAction != null)
                await invokeContext.PostAction();

            return result;
        }
    }

    public class InvokeDbActionContext
    {
        public Func<Task> PostAction { get; set; }

        public int Iter { get; set; }
    }
}
