using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace DBEngine
{
    public static class DbExtensions
    {
        public static string GetNullString(this DbDataReader r, int ordinal)
        {
            return r.IsDBNull(ordinal) ? "" : r.GetString(ordinal);
        }

        public static DateTime GetNullDateTime(this DbDataReader r, int ordinal)
        {
            return r.IsDBNull(ordinal) ? DateTime.MinValue : r.GetDateTime(ordinal);
        }
        public static T Def<T>(this DbDataReader r, int ord)
        {
            if (r.IsDBNull(ord)) return default;
            var t = r.GetValue(ord);
            return (T)t;
        }

        public static T? Val<T>(this DbDataReader r, int ord) where T : struct
        {
            if (r.IsDBNull(ord)) return default;
            var t = r.GetValue(ord);
            return (T)t;
        }

        public static T Ref<T>(this DbDataReader r, int ord) where T : class
        {
            if (r.IsDBNull(ord)) return default;
            var t = r.GetValue(ord);
            return (T)t;
        }
    }
}
