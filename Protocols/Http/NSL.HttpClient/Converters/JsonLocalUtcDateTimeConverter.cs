using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NSL.HttpClient.Converters
{
    public class JsonLocalUtcDateTimeConverter : JsonConverter<DateTime>
    {
        public static JsonLocalUtcDateTimeConverter Instance { get; } = new JsonLocalUtcDateTimeConverter();

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTime = reader.GetDateTime();
            return dateTime.ToLocalTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime());
        }
    }
}
