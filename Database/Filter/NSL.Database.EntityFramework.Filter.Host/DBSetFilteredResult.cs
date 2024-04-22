using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public class DBSetFilteredResult<T>
        where T : class
    {
        public IQueryable<T> CountQuery { get; set; }

        public IQueryable<T> Data { get; set; }

        public long Count => CountQuery.LongCount();

        public FilterResultModel<T> GetDataResult()
            => new FilterResultModel<T> { Data = Data.ToArray(), Count = Count };

        public async Task<FilterResultModel<T>> GetDataResultAsync()
            => new FilterResultModel<T> { Data = await Data.ToArrayAsync(), Count = await CountQuery.LongCountAsync() };

        public async Task<FilterResultModel<TType>> GetDataResultAsync<TType>(IQueryable<TType> dataQuery)
            => new FilterResultModel<TType> { Data = await dataQuery.ToArrayAsync(), Count = await CountQuery.LongCountAsync() };
    }

}
