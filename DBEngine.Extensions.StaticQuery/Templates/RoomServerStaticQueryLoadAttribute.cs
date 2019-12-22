using System;
using System.Collections.Generic;
using System.Text;

namespace ServerOptions.Extensions.StaticQuery.Templates
{
    public class RoomServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public RoomServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
