using System;

namespace BinarySerializer.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class BinaryAttribute: Attribute
    {
        public BinaryAttribute()
        {
        }

        public BinaryAttribute(Type type)
        {
        }
    }
}
