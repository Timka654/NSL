using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using NSL.ASPNET;
using Microsoft.Extensions.DependencyInjection;

namespace NSL.Database.EntityFramework.ASPNET
{
    public static class Extensions
    {
        public delegate Task<bool> InvokeDelegate<TContext>(TContext db, int n);

        public static async Task<bool> InvokeDbTransactionAsync<TContext>(this IServiceProvider services, InvokeDelegate<TContext> action)
            where TContext : DbContext
        {
            bool result = false;

            await services.InvokeInScopeAsync(async provider =>
            {
                result = await provider.GetRequiredService<TContext>().InvokeInTransactionAsync(action);
            });

            return result;
        }

        public static async Task<bool> InvokeInTransactionAsync<TContext>(this TContext context, InvokeDelegate<TContext> action)
            where TContext : DbContext
        {
            int i = 0;

            while (true)
            {
                try
                {
                    using var t = await context.Database.BeginTransactionAsync();

                    if (await action(context, i++))
                    {
                        await context.SaveChangesAsync();

                        await t.CommitAsync();

                        return true;
                    }

                    break;
                }
                catch (DbUpdateConcurrencyException dbu)
                {
                    foreach (var entry in dbu.Entries)
                    {
                        if (entry.State == EntityState.Deleted || entry.OriginalValues == null)
                            //When EF deletes an item its state is set to Detached
                            //http://msdn.microsoft.com/en-us/data/jj592676.aspx
                            context.Entry(entry.Entity).State = EntityState.Detached;
                        else
                        {
                            var dbv = entry.GetDatabaseValues();
                            if (dbv != null && entry.State == EntityState.Modified)
                                entry.OriginalValues.SetValues(dbv);
                            else if (dbv == null && entry.State != EntityState.Deleted)
                                entry.State = EntityState.Added;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return false;
        }
    }
}
