using NSL.Extensions.DataEngine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Extensions.DataEngine
{
    /// <summary>
    /// Experimental
    /// </summary>
    public class CacheManager
    {
        private Dictionary<Type, ICacheDataProvider> providers = new Dictionary<Type, ICacheDataProvider>();

        public CacheManager()
        {

        }

        public (Type, bool)[] RegisterProvider(ICacheDataProvider provider)
            => provider.GetCacheProviderType().Select(t => (t, providers.TryAdd(t, provider))).ToArray();

        public List<object> LoadData(Type type, DateTime? latestModified)
        {
            if (!providers.TryGetValue(type, out var provider))
                return null;

            return null;
        }
    }
}
