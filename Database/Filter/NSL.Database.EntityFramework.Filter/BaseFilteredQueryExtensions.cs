using NSL.Database.EntityFramework.Filter.Enums;
using NSL.Database.EntityFramework.Filter.Models;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Database.EntityFramework.Filter
{
    public static class BaseFilteredQueryExtensions
    {
        public static EntityFilterQueryModel AddFilter<T>(this EntityFilterQueryModel value, string propertyPath, CompareType compareType, T v)
        {
            var block = value.FilterQuery?.LastOrDefault();

            if (block == null)
            {
                value.AddFilterBlock();
                block = value.FilterQuery.LastOrDefault();
            }

            block.Properties.Add(new FilterPropertyViewModel() { PropertyPath = propertyPath, CompareType = compareType, Value = v.ToString() });
            return value;

        }

        public static EntityFilterQueryModel AddFilterBlock(this EntityFilterQueryModel value)
        {
            if (value.FilterQuery == null)
                value.FilterQuery = new List<EntityFilterBlockModel>() { };

            value.FilterQuery.Add(new EntityFilterBlockModel());

            return value;
        }

        public static EntityFilterQueryModel AddOrder(this EntityFilterQueryModel value, string propertyPath, bool asc = true)
        {
            var block = value.OrderQuery?.LastOrDefault();

            if (block == null)
            {
                value.AddOrderBlock();
                block = value.OrderQuery.LastOrDefault();
            }

            block.Properties.Add(new FilterPropertyOrderViewModel() { PropertyPath = propertyPath, ASC = asc });
            return value;

        }

        public static EntityFilterQueryModel AddOrderBlock(this EntityFilterQueryModel value)
        {
            if (value.OrderQuery == null)
                value.OrderQuery = new List<EntityFilterOrderBlockModel>() { };

            value.OrderQuery.Add(new EntityFilterOrderBlockModel());
            return value;
        }
    }
}
