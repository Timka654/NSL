using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevExtensions.Blazor.Http.HttpContent
{
    public class JsonHttpContent : StringContent
    {
        public static JsonSerializerOptions DefaultJsonOptions { get; } = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
        };

        public static JsonHttpContent Create<T>(T obj)
            => Create(obj, DefaultJsonOptions);

        public static JsonHttpContent Create<T>(T obj, JsonSerializerOptions options)
            => new JsonHttpContent(obj, options);

        private JsonHttpContent(object content, JsonSerializerOptions options)
            : base(JsonSerializer.Serialize(content, options), Encoding.UTF8, "application/json")
        {

        }
    }
}
