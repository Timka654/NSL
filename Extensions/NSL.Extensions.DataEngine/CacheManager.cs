using NSL.Extensions.DataEngine.Interface;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool RegisterProvider(ICacheDataProvider provider)
        {
            return providers.TryAdd(provider.GetType(), provider);
        }

        public List<object> LoadData(Type type, DateTime? latestMidified)
        {
            if(!providers.TryGetValue(type, out var provider) )
                return null;

            return null;
        }
    }
}
