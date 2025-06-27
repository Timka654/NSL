using NSL.Database.EntityFramework.Filter.Enums;
using System.Text.Json.Serialization;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class FilterPropertyViewModel : EntityFilterBasePropertyModel
    {
        public CompareType CompareType { get; set; }

        public bool InvertCompare { get; set; }

        public string Value { get; set; }

        public bool ValueNull { get; set; }

        public EntityFilterBlockModel ValueBlock { get; set; }
    }
}
