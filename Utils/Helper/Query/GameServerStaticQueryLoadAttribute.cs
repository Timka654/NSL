using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Helper.Query
{
    public class GameServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public GameServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
