using System;
using System.Reflection;

namespace NSL.Extensions.BinarySerializer
{
    public abstract class BinaryType
    {
        public BinaryStorage Storage { get; set; }

        public Type Type { get; set; }

        public abstract void RegisterScheme(BinaryScheme scheme);

        public abstract BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null);

        public abstract BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null);
    }
}
