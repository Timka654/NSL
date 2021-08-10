using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BinarySerializer
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

    public abstract class BinaryType
    {
        public BinaryStorage Storage { get; set; }

        public Type Type { get; set; }

        public abstract void RegisterScheme(BinaryScheme scheme);

        public abstract BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null);

        public abstract BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null);
    }

    public class BinaryTypeStorage : BinaryType
    {
        public BinaryReadAction ReadAction;
        public BinaryWriteAction WriteAction;

        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return ReadAction;
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            return WriteAction;
        }

        public override void RegisterScheme(BinaryScheme scheme)
        {

        }
    }

    public class BinaryTypeBasic : BinaryType
    {
        public override void RegisterScheme(BinaryScheme scheme)
        { 
        
        }

        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return null;
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            return null;
        }
    }

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
                return s.GetWriteAction(scheme,property);

            throw new Exception($"Cannot found binary struct for write {Type} with scheme {scheme}");
        }

        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            if (Store.TryGetValue(scheme, out var s))
                return s.GetReadAction(scheme, property);
            throw new Exception($"Cannot found binary struct for read {Type} with scheme {scheme}");
        }
    }

    public class BinaryScheme
    {
        public Type Type { get; set; }

        public string Scheme { get; set; }

        public BinaryWriteAction WriteAction { get; set; }

        public BinaryReadAction ReadAction { get; set; }
    }
}
