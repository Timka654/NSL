using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Context
{
    public abstract class EntityListComponentContext<TEntity, TRequest> : IPageableListComponentContext<TEntity>, IInitializingComponentContext, IUpdatableComponentContext
        where TRequest : class
    {
        public event Action OnUpdate = () => { };

        public void Update()
            => OnUpdate();

        public List<TEntity> Items { get; protected set; }

        public long ItemsCount { get; protected set; }

        public int ItemsPage { get; protected set; }

        public virtual int ItemsPerPage { get; } = 25;

        public TRequest Request { get; protected set; }

        public async ValueTask<bool> InitializeAsync()
        {
            if (ItemsPage > 0) return true;

            return await LoadItems(1, null, true);
        }

        public async ValueTask<bool> LoadItems(int page, TRequest request, bool requiredUpdate = false)
        {
            if (page == ItemsPage
                && object.Equals(Request, request)
                && !requiredUpdate)
                return true;


            var result = await LoadItemsInternal(page, request);

            if (result == default) return false;

            Request = request;
            ItemsPage = page;
            Items = result.Value.Items;
            ItemsCount = result.Value.ItemsCount;
            Update();
            return true;
        }

        protected abstract ValueTask<(List<TEntity> Items, long ItemsCount)?> LoadItemsInternal(int page, TRequest request);

        public virtual ValueTask<bool> LoadItems(int page, int skip, int take, TRequest request = null)
            => LoadItems(page, request, false);
    }

    public abstract class EntityListComponentContext<TEntity> : EntityListComponentContext<TEntity, object>;
}
