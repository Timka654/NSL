using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Threading;

namespace NSL.ASPNET.Configuration
{
    public class DbConfigurationProvider<TContext, TEntity> : ConfigurationProvider
        where TContext : DbContext
        where TEntity : DbConfigurationItemModel, new()
    {
        private readonly DbConfigurationOptions<TContext> options;

        private Timer reloadTimer;

        internal DbConfigurationProvider(DbConfigurationOptions<TContext> options)
        {
            this.options = options;
        }

        public override void Load()
        {
            reload(true);

            if (reloadTimer == null)
                ProcessReloadTimer();
        }

        DateTime lastLoad = DateTime.MinValue;

        private void reload(bool firstLoad = false)
        {
            using var dbContext = options.DBContextGetter(options.DBOptions);

            var set = dbContext.Set<TEntity>();

            var dt = DateTime.UtcNow;

            if (firstLoad || set.Any(x => x.UpdateTime > lastLoad) || set.Count() != Data.Count)
            {
                Data = set.ToDictionary(x => x.Name, x => x.Value);
                lastLoad = dt;

                if (!firstLoad)
                    base.OnReload();
            }
        }

        private void ProcessReloadTimer()
        {
            if (options.CheckUpdatesDelay.HasValue)
            {
                var delay = options.CheckUpdatesDelay.Value;

                DateTime now = DateTime.UtcNow;
                DateTime nextReloadTime = CalculateNextReloadTime(now, delay);

                // Рассчитайте оставшееся время до первой перезагрузки
                TimeSpan timeUntilNextReload = nextReloadTime - now;

                reloadTimer = new Timer(e => reload(), null, timeUntilNextReload, delay);
            }
        }

        private static DateTime CalculateNextReloadTime(DateTime currentTime, TimeSpan reloadInterval)
        {
            long ticks = (currentTime.Ticks + reloadInterval.Ticks - 1) / reloadInterval.Ticks * reloadInterval.Ticks;
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        public override async void Set(string key, string? value)
        {
            var allowUpdateTask = options.AllowUpdate(key);

            allowUpdateTask.Wait();

            int updated = 0;

            var ut = DateTime.UtcNow;

            var dbContext = options.DBContextGetter(options.DBOptions);

            var cset = dbContext.Set<TEntity>();

            if (allowUpdateTask.Result)
            {
                updated = await cset
                    .Where(x => x.Name == key)
                    .ExecuteUpdateAsync(x => x
                    .SetProperty(x => x.Value, value)
                    .SetProperty(x => x.UpdateTime, ut));
            }

            if (!allowUpdateTask.Result || updated == 0)
            {
                var createTask = options.AllowCreate(key);

                createTask.Wait();

                if (createTask.Result)
                {
                    if (allowUpdateTask.Result || !await cset.AnyAsync(x => x.Name == key))
                    {
                        try
                        {
                            cset.Add(new TEntity() { Name = key, Value = value, UpdateTime = ut });

                            await dbContext.SaveChangesAsync();
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }

            await dbContext.DisposeAsync();

            //base.Set(key, value);
        }
    }
}
