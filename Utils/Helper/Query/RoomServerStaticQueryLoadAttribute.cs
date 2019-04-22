using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.Query
{
    public class RoomServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public RoomServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
