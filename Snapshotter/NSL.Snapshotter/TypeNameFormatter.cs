using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Snapshotter
{
    public static class TypeNameFormatter
    {
        private static readonly Dictionary<Type, string> Aliases = new()
        {
            [typeof(string)] = "string",
            [typeof(int)] = "int",
            [typeof(long)] = "long",
            [typeof(short)] = "short",
            [typeof(byte)] = "byte",
            [typeof(bool)] = "bool",
            [typeof(object)] = "object",
            [typeof(decimal)] = "decimal",
            [typeof(double)] = "double",
            [typeof(float)] = "float",
            [typeof(Guid)] = "Guid",
            [typeof(DateTime)] = "DateTime",
            [typeof(DateTimeOffset)] = "DateTimeOffset"
        };

        public static string Format(Type? t)
        {
            if (t == null) return "null";

            // Nullable<T> -> T?
            var underlying = Nullable.GetUnderlyingType(t);
            if (underlying != null)
                return $"{Format(underlying)}?";

            if (Aliases.TryGetValue(t, out var a))
                return a;

            if (t.IsArray)
                return $"{Format(t.GetElementType()!)}[]";

            if (t.IsGenericType)
            {
                var def = t.GetGenericTypeDefinition();
                var name = def.Name;
                var tick = name.IndexOf('`');
                if (tick >= 0) name = name[..tick];

                if (def == typeof(List<>)) return $"{Format(t.GetGenericArguments().First())}[]";

                var args = t.GetGenericArguments().Select(Format);
                var prefix = t.Namespace is null ? name : $"{t.Namespace}.{name}";
                return $"{prefix}<{string.Join(",", args)}>";
            }

            // Nested types: use '.' not '+'
            var full = t.FullName ?? t.Name;
            return full.Replace('+', '.');
        }
    }

}
