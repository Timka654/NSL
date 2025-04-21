using System.Threading;
using System.Threading.Tasks;

namespace NSL.DataSource.ASPNET
{
    public interface IRestoreDataSource
    {
        Task RemoveDataAsync(string name, CancellationToken cancellationToken);

        Task SetDataAsync(string name, object data, CancellationToken cancellationToken);

        Task<TData?> TryGetDataAsync<TData>(string name, CancellationToken cancellationToken);
    }
}
