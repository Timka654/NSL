using System;
using System.Collections.Generic;
using System.Text;

namespace ServerOptions.Extensions.StaticQuery.Templates
{
    public class GameServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public GameServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
