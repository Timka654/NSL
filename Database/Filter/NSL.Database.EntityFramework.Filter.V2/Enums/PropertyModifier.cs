using System.Text.Json.Serialization;

namespace NSL.Database.EntityFramework.Filter.V2.Enums
{

namespace NSL.Database.EntityFramework.Filter.V2.Enums
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum PropertyModifier
        {
            /// <summary>
            /// No modifier is applied to the property.
            /// </summary>
            None,

            /// <summary>
            /// Applies a .Count() to the collection property.
            /// </summary>
            Count
        }
    }
}