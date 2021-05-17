using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinarySerializer.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class BinarySchemeAttribute : Attribute, IComparable<BinarySchemeAttribute>
    {
        public string Scheme { get; }

        public BinarySchemeAttribute(string scheme)
        {
            Scheme = scheme;
        }

        public int CompareTo(BinarySchemeAttribute other)
        {
            return Scheme.CompareTo(other.Scheme);
        }
    }
}
