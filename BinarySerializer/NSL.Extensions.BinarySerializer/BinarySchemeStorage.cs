using System;
using System.Collections.Generic;
using System.Reflection;

namespace NSL.Extensions.BinarySerializer
{
    public class BinarySchemeStorage : BinaryType
    {
        protected Dictionary<string, BinaryType> Store = new Dictionary<string, BinaryType>();

        public bool ExistsScheme(string scheme) => Store.ContainsKey(scheme);

        public override void RegisterScheme(BinaryScheme scheme)
        {
        }

        public void RegisterScheme(string scheme, BinaryType type)
        {
            if (!Store.TryGetValue(scheme, out var storage))
            {
                storage = type;
                Store.Add(scheme, storage);
            }
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            if (Store.TryGetValue(scheme, out var s))
                return s.GetWriteAction(scheme, property);

            throw new Exception($"Cannot found binary struct for write {Type} with scheme {scheme}");
        }

        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            if (Store.TryGetValue(scheme, out var s))
                return s.GetReadAction(scheme, property);
            throw new Exception($"Cannot found binary struct for read {Type} with scheme {scheme}");
        }
    }
}
