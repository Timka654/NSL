using System;
using System.Collections.Generic;
using System.Text;

namespace ServerOptions.Extensions.StaticQuery.Templates
{
    public class LoginServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public LoginServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
