using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.StructBuilder
{
    public class RoomServerStructBuilderLoadAttribute : StructBuilderLoadAttribute
    {
        public RoomServerStructBuilderLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
