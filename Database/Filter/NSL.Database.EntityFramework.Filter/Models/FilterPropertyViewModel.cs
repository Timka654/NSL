using NSL.Database.EntityFramework.Filter.Enums;
using System.Text.Json.Serialization;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public abstract class BasePropertyViewModel
    {
        private string propertyPath;

        [JsonIgnore]
        public PropertyDescriptorModel? PropertyInfo { get; set; }

        [JsonIgnore]
        public int Index { get; set; }

        public string PropertyPath { get => propertyPath ?? PropertyInfo?.Path; set => propertyPath = value; }
    }

    public class FilterPropertyViewModel : BasePropertyViewModel
    {

        public CompareType CompareType { get; set; }

        public string Value { get; set; }

        public bool ValueNull { get; set; }
    }
}
