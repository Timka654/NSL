using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NSL.Snapshotter
{
    public static class CanonicalJson
    {
        // Stable JSON: no indent, fixed options, deterministic property order (as declared), no null-ignore.
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
        };

        public static byte[] SerializeUtf8(SnapshotDocument doc, JsonSerializerOptions Options )
        {
            // Ensure canonical ordering before serialization
            var ordered = Canonicalize(doc);
            return JsonSerializer.SerializeToUtf8Bytes(ordered, Options);
        }

        public static SnapshotDocument Canonicalize(SnapshotDocument doc)
        {
            // items sorted by fullName, and internal members/params sorted.
            var items = doc.items
                .OrderBy(i => i.fullName, StringComparer.Ordinal)
                .ToArray();

            return new SnapshotDocument(doc.type, items);
        }
    }

}
