using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.StructBuilder
{
    public class LobbyServerStructBuilderLoadAttribute : StructBuilderLoadAttribute
    {
        public LobbyServerStructBuilderLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
