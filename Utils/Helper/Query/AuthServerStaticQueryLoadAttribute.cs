using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.Query
{
    public class AuthServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public AuthServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
