using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Context
{
    public abstract class EntityListComponentContext<TEntity> : IPageableListComponentContext<TEntity>, IInitializingComponentContext, IUpdatableComponentContext 
    {
        public event Action OnUpdate = () => { };

        public void Update()
            => OnUpdate();

        public List<TEntity> Items { get; protected set; }

        public long ItemsCount { get; protected set; }

        public int ItemsPage { get; protected set; }

        public virtual int ItemsPerPage { get; } = 25;

        public async Task InitializeAsync()
        {
            if (ItemsPage > 0) return;

            await LoadItems(1, true);
        }

        public async Task LoadItems(int page, bool requiredUpdate = false)
        {
            if (page == ItemsPage && !requiredUpdate)
                return;

            var result = await LoadItemsInternal(page);

            if (result == default) return;

            ItemsPage = page;
            Items = result.Value.Items;
            ItemsCount = result.Value.ItemsCount;
        }

        protected abstract Task<(List<TEntity> Items, long ItemsCount)?> LoadItemsInternal(int page);

        public virtual Task LoadItems(int page, int skip, int take)
            => LoadItems(page, false);
    }
}
