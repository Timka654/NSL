using System;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public static class FilterExtensions
    {
        public static bool ContainsCase(this string text, string search)
            => text.Contains(search);

        public static bool ContainsIgnoreCase(this string text, string search)
            => text.Contains(search, StringComparison.OrdinalIgnoreCase);

        public static bool StartsWithCase(this string text, string search)
            => text.StartsWith(search);

        public static bool StartsWithIgnoreCase(this string text, string search)
            => text.StartsWith(search, StringComparison.OrdinalIgnoreCase);

        public static bool EndsWithCase(this string text, string search)
            => text.EndsWith(search);

        public static bool EndsWithIgnoreCase(this string text, string search)
            => text.EndsWith(search, StringComparison.OrdinalIgnoreCase);
    }
}
