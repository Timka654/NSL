using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.StructBuilder
{
    public class GameServerStructBuilderLoadAttribute : StructBuilderLoadAttribute
    {
        public GameServerStructBuilderLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
