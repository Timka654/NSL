using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.Query
{
    public class ClientServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public ClientServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
