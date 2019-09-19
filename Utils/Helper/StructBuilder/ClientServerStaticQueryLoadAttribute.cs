using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.StructBuilder
{
    public class ClientServerStaticQueryLoadAttribute : StructBuilderLoadAttribute
    {
        public ClientServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
