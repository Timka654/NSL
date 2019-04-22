using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.Query
{
    public class LoginServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public LoginServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
