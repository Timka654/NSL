using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinarySerializer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BinaryPreCompileAttribute : Attribute
    {
        public int InitialSize { get; private set; } = 32;

        public string Scheme { get; private set; }

        public BinaryPreCompileAttribute(string scheme, int initialSize = 32)
        {
            InitialSize = initialSize;
            Scheme = scheme;

        }
    }
}
