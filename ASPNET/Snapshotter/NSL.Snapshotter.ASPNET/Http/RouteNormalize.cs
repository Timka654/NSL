using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Snapshotter.ASPNET.Http
{
    public static class RouteNormalize
    {
        public static string NormalizeToAbsolute(string? controllerRoute, string? actionRoute)
        {
            var a = (controllerRoute ?? "").Trim();
            var b = (actionRoute ?? "").Trim();

            // basic join
            var url = a.Length == 0 ? b : (b.Length == 0 ? a : $"{a}/{b}");

            // ensure leading slash
            if (!url.StartsWith("/", StringComparison.Ordinal))
                url = "/" + url;

            // collapse multiple slashes
            while (url.Contains("//", StringComparison.Ordinal))
                url = url.Replace("//", "/", StringComparison.Ordinal);

            // normalize trailing slash: keep only root "/"
            if (url.Length > 1 && url.EndsWith("/", StringComparison.Ordinal))
                url = url.TrimEnd('/');

            return url.Length == 0 ? "/" : url;
        }

        /// <summary>
        /// Extracts route params from a template, returning tokens as {name} in the normalizedUrl,
        /// plus param list with optional type from {name:type}.
        /// </summary>
        public static (string normalizedUrl, List<RouteParamSnapshot> paramList) NormalizeParams(
            string url,
            IReadOnlyList<(string name, Type type)> actionParamsByName
        )
        {
            var paramList = new List<RouteParamSnapshot>();

            // Find {...} tokens manually (simple and enough for v0)
            // Keep "{name}" in URL; extract type if "{name:type}" or "{name:regex(...)}" -> type="regex"
            var sb = new StringBuilder(url.Length);
            for (int i = 0; i < url.Length; i++)
            {
                var ch = url[i];
                if (ch != '{')
                {
                    sb.Append(ch);
                    continue;
                }

                var end = url.IndexOf('}', i + 1);
                if (end < 0)
                {
                    sb.Append(ch);
                    continue;
                }

                var inside = url.Substring(i + 1, end - i - 1).Trim();
                // examples: "id", "id:guid", "slug:regex(^[a-z]+$)"
                var name = inside;
                string? type = null;

                var colon = inside.IndexOf(':');
                if (colon >= 0)
                {
                    name = inside[..colon].Trim();
                    var rawType = inside[(colon + 1)..].Trim();
                    var paren = rawType.IndexOf('(');
                    type = (paren >= 0 ? rawType[..paren] : rawType).Trim();
                    if (type.Length == 0) type = null;
                    else type = type.ToLowerInvariant();
                }

                // if type not in template, try from action signature by name
                if (type == null && name.Length > 0)
                {
                    var match = actionParamsByName.FirstOrDefault(p =>
                        string.Equals(p.name, name, StringComparison.OrdinalIgnoreCase));
                    if (match.name is not null)
                        type = TypeNameFormatter.Format(match.type);
                }

                // record param always (as we agreed: keep param list, type may be null)
                if (name.Length > 0)
                    paramList.Add(new RouteParamSnapshot(name, type));

                sb.Append('{').Append(name).Append('}');
                i = end; // skip to '}'
            }

            return (sb.ToString(), paramList
                .OrderBy(p=>p.name, StringComparer.Ordinal)
                .ToList());
        }
    }

}
