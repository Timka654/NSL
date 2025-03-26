using NSL.Database.EntityFramework.Filter.Enums;
using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.Models
{
    /// <summary>
    /// It's OR Where block
    /// </summary>
    public class EntityFilterBlockModel
    {
        /// <summary>
        /// Where And compare properties values
        /// </summary>
        public List<FilterPropertyViewModel> Properties { get; set; } = new List<FilterPropertyViewModel>();

        public EntityFilterBlockModel AddProperty<TValue>(string propertyPath, CompareType compareType, TValue value)
            => AddProperty(propertyPath, compareType, (object)value);

        public EntityFilterBlockModel AddProperty(string propertyPath, CompareType compareType, object value)
        {
            Properties.Add(new FilterPropertyViewModel()
            {
                PropertyPath = propertyPath,
                CompareType = compareType,
                ValueNull = value == null,
                Value = value?.ToString()
            });

            return this;
        }
    }
}
