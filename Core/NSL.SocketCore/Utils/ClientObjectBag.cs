using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketCore.Utils
{
    public class ClientObjectBag : IDisposable
    {
        private ConcurrentDictionary<string, object> store = new ConcurrentDictionary<string, object>();

        public object this[string key]
        {
            get
            {
                store.TryGetValue(key, out object value);
                return value;
            }
            set
            {
                if(!store.TryAdd(key, value))
                    store[key] = value;
            }
        }

        public object Get(string key) => this[key];

        public object Get<TObject>(string key) => (TObject)this[key];

        public void Set(string key, object value) => this[key] = value;

        public void Set<TObject>(string key, TObject value) => this[key] = value;

        public void Remove(string key) => store.TryRemove(key, out object _);

        public bool Exists(string key) => store.ContainsKey(key);

        public void Dispose()
        {
            store.Clear();
        }
    }
}
