using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.Query
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StaticQueryAttribute : Attribute
    {
        public int Order { get; private set; }

        public string Name { get; private set; }

        public StaticQueryAttribute(int order, string name)
        {
            Order = order;
            Name = name;
        }
    }
}
