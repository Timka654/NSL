using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSL.ASPNET;
using NSL.ASPNET.Attributes;
using NSL.ASPNET.Services;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.ASPNET
{
    public class CacheItemModel<TItem>
    {
        public TItem Item { get; init; }
        public DateTime LastCallTime { get; set; } = DateTime.UtcNow;
    }

    public abstract class CacheItemService<TKey, TItem, TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        [RegisterServiceConstructor]
        public CacheItemService(IServiceProvider serviceProvider, NSLRootTimerService nslRootTimerService) : this(serviceProvider)
        {
            nslRootTimerService.Tick += nslRootTimerService_Tick;
        }

        public virtual TimeSpan AccessCacheTime { get; } = TimeSpan.FromMinutes(10);

        TimeSpan lastCleanTime = TimeSpan.Zero;
        private Task nslRootTimerService_Tick(long iter, TimeSpan iterTime, CancellationToken cancellationToken) => Task.Run(() =>
        {
            lastCleanTime = lastCleanTime + iterTime;

            if (lastCleanTime < AccessCacheTime)
            {
                return;
            }

            lastCleanTime = TimeSpan.Zero;

            var limit = DateTime.UtcNow - (iterTime * 2);

            var toRemove = cacheItems
            .Where(kv => kv.Value.LastCallTime < limit)
            .Select(kv => kv.Key)
            .ToArray();

            foreach (var id in toRemove)
                cacheItems.TryRemove(id, out _);
        }, cancellationToken);

        ConcurrentDictionary<TKey, CacheItemModel<TItem>> cacheItems = new();

        public void Clear(TKey id) => cacheItems.TryRemove(id, out _);

        public void Clear(Func<TItem, bool> condition)
        {
            foreach (var item in cacheItems
                .Where(x => !Equals(x.Value.Item, default(TItem)) && condition(x.Value.Item))
                .Select(x => x.Key)
                .ToArray())
            {
                Clear(item);
            }
        }

        public async Task<TItem?> GetOrLoadAsync(TKey id
            , TContext? dbContext = null)
        {
            if (cacheItems.TryGetValue(id, out var settings))
            {
                settings.LastCallTime = DateTime.UtcNow;

                return settings.Item;
            }

            if (dbContext != default)
                return (await Load(id, dbContext)).Item;

            await serviceProvider.InvokeDbTransactionAsync<TContext>(async db =>
            {
                settings = await Load(id, db);

                return false;
            });

            return settings!.Item;
        }

        async Task<CacheItemModel<TItem>> Load(TKey id
            , TContext? dbContext)
        {
            var loaded = new CacheItemModel<TItem>()
            {
                Item = await LoadItem(id, dbContext)
            };

            cacheItems.TryAdd(id, loaded);

            return loaded;
        }

        protected abstract Task<TItem> LoadItem(TKey id
            , TContext? dbContext);
    }
    public abstract class CacheItemService<TItem, TContext> : CacheItemService<Guid, TItem, TContext>
        where TContext : DbContext
    {
        [RegisterServiceConstructor]
        public CacheItemService(IServiceProvider serviceProvider, NSLRootTimerService accessCleanTimerService) : base(serviceProvider, accessCleanTimerService)
        {
        }
    }
}
