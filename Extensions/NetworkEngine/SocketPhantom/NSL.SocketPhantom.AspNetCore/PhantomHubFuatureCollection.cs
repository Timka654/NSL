using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NSL.SocketPhantom.AspNetCore
{
    public class PhantomHubFuatureCollection : IFeatureCollection
    {
        private ConcurrentDictionary<Type, object> fuatures = new ConcurrentDictionary<Type, object>();

        public object this[Type key]
        {
            get => fuatures.TryGetValue(key, out var result) ? result : default;
            set
            {
                if (fuatures.ContainsKey(key))
                {
                    fuatures[key] = value;
                    return;
                }
                fuatures.TryAdd(key, value);
            }
        }

        public bool IsReadOnly => false;

        public int Revision => 1;

        public TFeature Get<TFeature>()
        {
            var result = this[typeof(TFeature)];

            if (result == null)
                return default;

            return (TFeature)result;
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return fuatures.GetEnumerator();
        }

        public void Set<TFeature>(TFeature instance)
        {
            this[typeof(TFeature)] = instance;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return fuatures.GetEnumerator();
        }
    }
}
