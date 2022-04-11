using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NSL.Extensions.BinarySerializer
{
    public delegate void BinaryWriteAction(MemoryStream ms, BinaryScheme scheme, dynamic obj);
    public delegate dynamic BinaryReadAction(MemoryStream ms, BinaryScheme scheme, dynamic obj);

    public class BinaryStorage
    {
        protected Dictionary<string, BinaryType> Store = new Dictionary<string, BinaryType>();

        public virtual void RegisterType(Type type, BinaryType binaryType)
        {
            if (!Store.TryGetValue(type.Name, out var storage))
            {
                Store.Add(type.Name, binaryType);
            }

            binaryType.Storage = this;
            binaryType.Type = type;
        }

        public virtual BinaryWriteAction GetWriteAction(Type type, PropertyInfo property, string scheme = "default")
        {
            if (Store.TryGetValue(type.Name, out var storage))
            {
                return storage.GetWriteAction(scheme, property);
            }

            throw new Exception($"Cannot found binary struct for write {type} with scheme {scheme}");
        }

        public virtual BinaryReadAction GetReadAction(Type type, PropertyInfo property, string scheme = "default")
        {
            if (Store.TryGetValue(type.Name, out var storage))
            {
                return storage.GetReadAction(scheme, property);
            }

            throw new Exception($"Cannot found binary struct for read {type} with scheme {scheme}");
        }

        public virtual bool ContainsType(Type type) => Store.ContainsKey(type.Name);

        public virtual bool ContainsType(Type type, string scheme)
        {
            if (Store.TryGetValue(type.Name, out var bt))
            {
                if (bt is BinarySchemeStorage s)
                {
                    return s.ExistsScheme(scheme);
                }
                return true;
            };
            return false;
        }

        public virtual BinarySchemeStorage GetBinarySchemeStorage(Type type)
        {
            if (Store.TryGetValue(type.Name, out var bt))
            {
                if (bt is BinarySchemeStorage s)
                {
                    return s;
                }

            };

            return null;
        }
    }

}
