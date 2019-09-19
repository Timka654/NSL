using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.StructBuilder
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StructBuilderLoadAttribute : Attribute
    {
        public int Order { get; private set; }

        public string Name { get; private set; }

        public StructBuilderLoadAttribute(int order, string name)
        {
            Order = order;
            Name = name;
        }
    }
}
