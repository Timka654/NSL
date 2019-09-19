using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.StructBuilder
{
    public class LoginServerStructBuilderLoadAttribute : StructBuilderLoadAttribute
    {
        public LoginServerStructBuilderLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
