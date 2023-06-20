using System;
using System.Collections.Generic;

namespace NSL.Extensions.DataEngine.Interface
{
    public interface ICacheDataProvider
    {
        Type[] GetCacheProviderType();
    }

    public interface ICacheDataLoader : ICacheDataProvider
    {
        IEnumerable<object> LoadCacheData(DateTime? latestUpdate);
    }

    public interface ICacheDataLoader<TEntity> : ICacheDataLoader
        where TEntity : ICacheDataEntry
    {
        IEnumerable<TEntity> LoadCacheData(DateTime? latestUpdate);
    }

    public interface ICacheDataReceiver<TEntity> : ICacheDataProvider
        where TEntity : ICacheDataEntry
    {
        void ReceiveCacheData(DateTime? newLatestUpdate, IEnumerable<TEntity> data);
    }
}
