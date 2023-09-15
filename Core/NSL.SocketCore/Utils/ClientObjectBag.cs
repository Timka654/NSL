using System;
using System.Collections.Concurrent;

namespace NSL.SocketCore.Utils
{
    public class ClientObjectBag : ObjectBag { }

    public class ObjectBag : IDisposable
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

        public TObject Get<TObject>(string key, bool throwIfNotExists = false)
        {
            var item = this[key];

            if (throwIfNotExists && item == default)
                throw new Exception($"Object bag not contains object with key equals \"{key}\"");

            return (TObject)item;
        }

        public void Set(string key, object value) => this[key] = value;

        public void Set<TObject>(string key, TObject value) => this[key] = value;

        public void Remove(string key) => store.TryRemove(key, out object _);

        public bool Exists(string key) => store.ContainsKey(key);

        public void Dispose()
        {
            foreach (var item in store)
            {
                if (item.Value is IDisposable d)
                    d.Dispose();
            }

            store.Clear();
        }
    }
}
