using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public class DBSetFilteredResult<T>
        where T : class
    {
        public IQueryable<T> CountQuery { get; set; }

        public IQueryable<T> Data { get; set; }

        public long Count => CountQuery.LongCount();

        public EntityFilterResultModel<T> GetDataResult()
            => new EntityFilterResultModel<T> { Data = Data.ToArray(), Count = Count };

        public async Task<EntityFilterResultModel<T>> GetDataResultAsync(CancellationToken cancellationToken = default)
            => new EntityFilterResultModel<T> { Data = await Data.ToArrayAsync(cancellationToken), Count = await CountQuery.LongCountAsync(cancellationToken) };

        public async Task<EntityFilterResultModel<TType>> GetDataResultAsync<TType>(IQueryable<TType> dataQuery, CancellationToken cancellationToken = default)
            => new EntityFilterResultModel<TType> { Data = await dataQuery.ToArrayAsync(cancellationToken), Count = await CountQuery.LongCountAsync(cancellationToken) };
    }

}
