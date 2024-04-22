using NSL.Database.EntityFramework.Filter.Enums;
using NSL.Database.EntityFramework.Filter.Models;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Database.EntityFramework.Filter
{
    public static class BaseFilteredQueryExtensions
    {
        public static BaseFilteredQueryModel AddFilter<T>(this BaseFilteredQueryModel value, string propertyPath, CompareType compareType, T v)
        {
            var block = value.FilterQuery?.LastOrDefault();

            if (block == null)
            {
                value.AddFilterBlock();
                block = value.FilterQuery.LastOrDefault();
            }

            block.Propertyes.Add(new FilterPropertyViewModel() { PropertyPath = propertyPath, CompareType = compareType, Value = v.ToString() });
            return value;

        }

        public static BaseFilteredQueryModel AddFilterBlock(this BaseFilteredQueryModel value)
        {
            if (value.FilterQuery == null)
                value.FilterQuery = new List<FilterBlockViewModel>() { };

            value.FilterQuery.Add(new FilterBlockViewModel());

            return value;
        }

        public static BaseFilteredQueryModel AddOrder(this BaseFilteredQueryModel value, string propertyPath, bool asc = true)
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

        public static BaseFilteredQueryModel AddOrderBlock(this BaseFilteredQueryModel value)
        {
            if (value.OrderQuery == null)
                value.OrderQuery = new List<FilterBlockOrderViewModel>() { };

            value.OrderQuery.Add(new FilterBlockOrderViewModel());
            return value;
        }
    }
}
