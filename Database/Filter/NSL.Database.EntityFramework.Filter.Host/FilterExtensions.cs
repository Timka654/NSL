using System;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public static class FilterExtensions
    {
        public  static bool ContainsCase(this string text, string search)
            => text.Contains(search);

        public  static bool ContainsIgnoreCase(this string text, string search)
            => text.Contains(search, StringComparison.OrdinalIgnoreCase);
    }
}
