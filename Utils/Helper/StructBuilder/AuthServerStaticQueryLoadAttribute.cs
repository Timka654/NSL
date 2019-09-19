using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.StructBuilder
{
    public class AuthServerStaticQueryLoadAttribute : StructBuilderLoadAttribute
    {
        public AuthServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
