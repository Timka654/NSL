using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinarySerializer
{
    public class BinaryPreCompileAttribute : Attribute
    {
        public int InitialSize { get; private set; } = 32;

        public string[] Schemes { get; private set; }

        public BinaryPreCompileAttribute(int initialSize = 32, params string[] schemes)
        {
            InitialSize = initialSize;
            if (schemes.Contains(""))
                Schemes = schemes.ToArray();
            else
                Schemes = schemes.Append("").ToArray();

        }
    }
}
