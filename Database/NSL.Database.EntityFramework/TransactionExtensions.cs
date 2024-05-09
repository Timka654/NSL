using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="context"></param>
        /// <param name="action">execute transaction actions - must return "true" for save, can return "false" for ignore, on "false" - DbUpdateConcurrencyException was cleared and not throw</param>
        /// <param name="maxCount">max available execution try count</param>
        /// <param name="onHasThrow">handle for concurrency exception - can clear exception for prevent throw</param>
        /// <exception cref="DbUpdateConcurrencyException">Throw on n == maxCount</exception>
        /// <returns></returns>
        public static async Task<bool> InvokeInTransactionAsync<TContext>(this TContext context, InvokeDelegate<TContext> action, int maxCount = 100, Func<DbUpdateConcurrencyException, DbUpdateConcurrencyException?> onHasThrow = null)
            where TContext : DbContext
        {
            int i = 0;

            DbUpdateConcurrencyException? latestEx = null;

            while (i < maxCount) // throw count limit for concurrent - m/b can contains logic error 
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

                    latestEx = null;

                    break;
                }
                catch (DbUpdateConcurrencyException dbu)
                {
                    latestEx = dbu;

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

                    if (onHasThrow != null)
                    {
                        latestEx = onHasThrow(dbu);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            if (latestEx != null)
                throw latestEx;

            return false;
        }
    }
}
