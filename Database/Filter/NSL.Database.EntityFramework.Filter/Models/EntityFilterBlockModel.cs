﻿using NSL.Database.EntityFramework.Filter.Enums;
using System.Collections.Generic;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class EntityFilterBlockModel
    {
        public List<FilterPropertyViewModel> Properties { get; set; } = new List<FilterPropertyViewModel>();

        public EntityFilterBlockModel AddFilter(string propertyPath, CompareType compareType, object value)
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
