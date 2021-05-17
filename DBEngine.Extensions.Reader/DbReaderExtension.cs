using System;
using System.Data.Common;

namespace DBEngine.Extensions.Reader
{
    public static class DbReaderExtension
    {
        /// <summary>
        /// Получить значение string с поля которое поддерживает значение NULL
        /// </summary>
        /// <param name="r"></param>
        /// <param name="ordinal"></param>
        /// <returns>string.Empty ?? value</returns>
        public static string GetNullString(this DbDataReader r, int ordinal)
        {
            return r.IsDBNull(ordinal) ? string.Empty : r.GetString(ordinal);
        }

        /// <summary>
        /// Получить значение DateTime с поля которое поддерживает значение NULL
        /// </summary>
        /// <param name="r"></param>
        /// <param name="ordinal"></param>
        /// <returns>DateTime.MinValue ?? Value</returns>
        public static DateTime GetNullDateTime(this DbDataReader r, int ordinal)
        {
            return r.IsDBNull(ordinal) ? DateTime.MinValue : r.GetDateTime(ordinal);
        }

        /// <summary>
        /// Получить значение указанного типа с поля которое поддерживает значение NULL
        /// </summary>
        /// <param name="r"></param>
        /// <param name="ordinal"></param>
        /// <returns>Value ?? default</returns>
        public static T Def<T>(this DbDataReader r, int ord)
        {
            if (r.IsDBNull(ord)) return default;
            var t = r.GetValue(ord);
            return (T)t;
        }

        /// <summary>
        /// Получить значение указанного типа с поля которое поддерживает значение NULL
        /// </summary>
        /// <param name="r"></param>
        /// <param name="ordinal"></param>
        /// <returns>Value ?? defaultValue</returns>
        public static T Def<T>(this DbDataReader r, int ord, T defaultValue)
        {
            if (r.IsDBNull(ord)) return defaultValue;
            var t = r.GetValue(ord);
            return (T)t;
        }

        /// <summary>
        /// Получить значение указанного типа с поля которое поддерживает значение NULL
        /// </summary>
        /// <param name="r"></param>
        /// <param name="ordinal"></param>
        /// <returns>default ?? Value</returns>
        public static T? Val<T>(this DbDataReader r, int ord) where T : struct
        {
            if (r.IsDBNull(ord)) return default;
            var t = r.GetValue(ord);
            return (T)t;
        }

        /// <summary>
        /// Получить значение указанного типа с поля которое поддерживает значение NULL
        /// </summary>
        /// <param name="r"></param>
        /// <param name="ordinal"></param>
        /// <returns>default ?? Value</returns>
        public static T Ref<T>(this DbDataReader r, int ord) where T : class
        {
            if (r.IsDBNull(ord)) return default;
            var t = r.GetValue(ord);
            return (T)t;
        }
    }
}
