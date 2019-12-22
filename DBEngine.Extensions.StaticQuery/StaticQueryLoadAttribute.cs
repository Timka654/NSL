using System;
using System.Collections.Generic;
using System.Text;

namespace ServerOptions.Extensions.StaticQuery
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StaticQueryLoadAttribute : Attribute
    {
        public int Order { get; private set; }

        public string Name { get; private set; }

        public StaticQueryLoadAttribute(int order, string name)
        {
            Order = order;
            Name = name;
        }
    }
}
