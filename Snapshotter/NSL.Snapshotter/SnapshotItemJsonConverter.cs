using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NSL.Snapshotter
{
    public sealed class SnapshotItemJsonConverter : JsonConverter<SnapshotItem>
    {
        private readonly SnapshotItemTypeRegistry _registry;

        public SnapshotItemJsonConverter(SnapshotItemTypeRegistry registry) => _registry = registry;

        public override SnapshotItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);

            if (!doc.RootElement.TryGetProperty("kind", out var kindProp))
                throw new JsonException("SnapshotItem missing 'kind'.");

            var kind = kindProp.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(kind))
                throw new JsonException("SnapshotItem 'kind' is empty.");

            var clrType = _registry.Resolve(kind);

            // Deserialize whole object into resolved type
            var json = doc.RootElement.GetRawText();
            return (SnapshotItem)(JsonSerializer.Deserialize(json, clrType, options)
                                  ?? throw new JsonException($"Failed to deserialize kind '{kind}'."));
        }

        public override void Write(Utf8JsonWriter writer, SnapshotItem value, JsonSerializerOptions options)
        {

            // Let STJ serialize the runtime type (it will include "kind" property from your DTO)
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }

}
