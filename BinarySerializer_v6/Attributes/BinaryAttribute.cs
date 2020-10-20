using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
