using NSL.Database.EntityFramework.Filter.Enums;
using System;
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

        public EntityFilterBlockModel AddProperty<TValue>(string propertyPath, CompareType compareType, TValue value, bool invert = false)
            => AddProperty(propertyPath, compareType, (object)value, invert);

        public EntityFilterBlockModel AddProperty(string propertyPath, CompareType compareType, Action<EntityFilterBlockModel> builder, bool invert = false)
        {
            if(builder == null)
                return AddProperty(propertyPath, compareType, (object)null, invert);

            EntityFilterBlockModel b = new EntityFilterBlockModel();

            builder(b);

            Properties.Add(new FilterPropertyViewModel()
            {
                PropertyPath = propertyPath,
                CompareType = compareType,
                ValueNull = false,
                ValueBlock = b,
                InvertCompare = invert
            });

            return b;
        }

        public EntityFilterBlockModel AddProperty(string propertyPath, CompareType compareType, object value, bool invert = false)
        {
            Properties.Add(new FilterPropertyViewModel()
            {
                PropertyPath = propertyPath,
                CompareType = compareType,
                ValueNull = value == null,
                Value = value?.ToString(),
                InvertCompare = invert
            });

            return this;
        }
    }
}
