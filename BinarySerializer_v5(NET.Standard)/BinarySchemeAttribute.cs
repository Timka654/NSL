using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class BinarySchemeAttribute : Attribute
    {
        public string SchemeName { get; private set; }

        public BinarySchemeAttribute(string schemeName)
        {
            SchemeName = schemeName;
        }
    }
}
