using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.Utils
{
    public class HasherUtils
    {
        public static int GetInt32HashCode(string strText)
            => strText.GetHashCode();

        public static int GetInt32HashCode(params string[] strText)
            => GetInt32HashCode(string.Concat(strText));
    }
}
