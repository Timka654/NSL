using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Context
{
    public interface IPageableListComponentContext<TEntity> : IEntityListComponentContext<TEntity>
    {

        long ItemsCount { get; }

        int ItemsPage { get; }

        int ItemsPerPage { get; }

        //ValueTask<bool> LoadItems(int page, bool requiredUpdate = false);

        //ValueTask<bool> LoadItems(int page, int skip, int take);
    }
}
