using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Enums.NSL.Database.EntityFramework.Filter.V2.Enums;
using System.Text.Json.Serialization;

namespace NSL.Database.EntityFramework.Filter.V2.Models
{
    public class EntityFilterBlockModel
    {
        /// <summary>
        /// Path to property for filter (e.g., "User.Name").
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// A modifier to apply to the property, e.g., getting the count of a collection.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public PropertyModifier Modifier { get; set; } = PropertyModifier.None;

        /// <summary>
        /// The filter operation to perform.
        /// </summary>
        public FilterOperator Type { get; set; }

        /// <summary>
        /// The value to compare against, represented as a string.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Indicates if string operations should be case-sensitive.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Invert the result of the filter operation.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Not { get; set; }

        /// <summary>
        /// A nested filter definition, used for 'Any' or conditional 'Count' operations on collection properties.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FilterNode NestedFilter { get; set; }
    }
}