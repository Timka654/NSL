using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Context
{
    public interface IPageableListComponentContext<TEntity> : IEntityListComponentContext<TEntity>
    {

        long ItemsCount { get; }

        int ItemsPage { get; }

        int ItemsPerPage { get; }

        Task LoadItems(int page, bool requiredUpdate = false);

        Task LoadItems(int page, int skip, int take);
    }
}
