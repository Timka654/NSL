using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BinaryAttribute : Attribute
    {
        public int TypeSize { get; set; } = 0;

        public string TypeSizeName { get; set; } = null;

        public int ArraySize { get; set; } = 0;

        public string ArraySizeName { get; set; } = null;

        public Type Type { get; set; }

        public BinaryAttribute(Type type)
        {
            Type = type;
        }
    }
}
