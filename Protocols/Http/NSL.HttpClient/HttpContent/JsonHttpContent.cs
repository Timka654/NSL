using NSL.HttpClient.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NSL.HttpClient.HttpContent
{
    public class JsonHttpContent : StringContent
    {
        public static JsonSerializerOptions DefaultJsonOptions => new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
        };

        public static JsonSerializerOptions AddJsonLocalUtcDateTimeConverterBuildAction(JsonSerializerOptions options)
        {
            options.Converters.Add(JsonLocalUtcDateTimeConverter.Instance);

            return options;
        }

        public static List<Func<JsonSerializerOptions, JsonSerializerOptions>> JsonOptionsBuilderActions { get; } = new List<Func<JsonSerializerOptions, JsonSerializerOptions>> {
            AddJsonLocalUtcDateTimeConverterBuildAction
        };

        public static JsonSerializerOptions BuildJsonOptions(JsonSerializerOptions? options)
        {
            JsonSerializerOptions result = options ?? DefaultJsonOptions;

            foreach (var item in JsonOptionsBuilderActions)
            {
                result = item(result);
            }

            return result;
        }

        public static JsonHttpContent Create<T>(T obj)
            => Create(obj, BuildJsonOptions(null));

        public static JsonHttpContent Create<T>(T obj, JsonSerializerOptions options)
            => new JsonHttpContent(obj, options);

        private JsonHttpContent(object content, JsonSerializerOptions options)
            : base(JsonSerializer.Serialize(content, options), Encoding.UTF8, "application/json")
        {

        }
    }
}
