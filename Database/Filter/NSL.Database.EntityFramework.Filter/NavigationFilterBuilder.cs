using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NSL.Database.EntityFramework.Filter.Enums;
using NSL.Database.EntityFramework.Filter.Models;

namespace NSL.Database.EntityFramework.Filter
{
    public class NavigationFilterBuilder : BaseFilteredQueryModel
    {
        private FilterBlockViewModel currentBlock = null;
        private FilterBlockOrderViewModel currentOrderBlock = null;

        private NavigationFilterBuilder()
        {
            FilterQuery = new List<FilterBlockViewModel>();
            OrderQuery = new List<FilterBlockOrderViewModel>();
        }

        public static NavigationFilterBuilder Create() => new NavigationFilterBuilder();

        public NavigationFilterBuilder AddPageOffset(int offset, int count)
        {
            Offset = offset;
            Count = count;
            return this;
        }

        public NavigationFilterBuilder SetOffsetFromPage(int page, int pageSize)
        {
            return AddPageOffset((int)Math.Ceiling((page - 1.0) * pageSize), pageSize);
        }

        public NavigationFilterBuilder AddFilter(string propertyPath, CompareType compareType, object value)
        {
            if (currentBlock == null)
                CreateFilterBlock();

            currentBlock.AddFilter(propertyPath, compareType, value);

            return this;
        }

        public NavigationFilterBuilder AddFilter<T>(string propertyPath, CompareType compareType, T value) => AddFilter(propertyPath, compareType, (object)value);

        public NavigationFilterBuilder CreateFilterBlock()
        {
            currentBlock = new FilterBlockViewModel();
            FilterQuery.Add(currentBlock);
            return this;
        }

        public NavigationFilterBuilder CreateFilterBlock(Action<FilterBlockViewModel> buildBlock)
        {
            var cb = new FilterBlockViewModel();
            buildBlock(cb);
            FilterQuery.Add(cb);
            return this;
        }


        public NavigationFilterBuilder AddOrder<T>(string propertyPath, bool asc = true)
        {
            if (currentOrderBlock == null)
                CreateOrderBlock();
            currentOrderBlock.Properties.Add(new FilterPropertyOrderViewModel() { PropertyPath = propertyPath, ASC = asc });
            return this;
        }

        public NavigationFilterBuilder CreateOrderBlock()
        {
            currentOrderBlock = new FilterBlockOrderViewModel();
            OrderQuery.Add(currentOrderBlock);
            return this;
        }

        public NavigationFilterBuilder CreateOrderBlock(Action<FilterBlockOrderViewModel> buildBlock)
        {
            var cb = new FilterBlockOrderViewModel();
            buildBlock(cb);
            OrderQuery.Add(cb);
            return this;
        }

        public NavigationFilterBuilder SetOffset(int offset)
        {
            Offset = offset;

            return this;
        }

        public NavigationFilterBuilder SetCount(int count)
        {
            Count = count;

            return this;
        }

        public static BaseFilteredQueryModel CreatePageOffsetFilter(int page, int pageSize)
        {
            return CreateOffsetFilter<BaseFilteredQueryModel>((int)Math.Ceiling((page - 1.0) * pageSize), pageSize);
        }

        public static T CreateOffsetFilter<T>(int offset, int pageSize)
            where T : BaseFilteredQueryModel, new()
        {
            return new T() { Count = pageSize, Offset = offset };
        }

        public static T CreatePageOffsetFilter<T>(int page, int pageSize)
            where T : BaseFilteredQueryModel, new()
        {
            return CreateOffsetFilter<T>((int)Math.Ceiling((page - 1.0) * pageSize), pageSize);
        }

        public string ToJson() => JsonSerializer.Serialize(this);

        public NavigationFilterBuilder ClearEmptyFilter()
        {
            FilterQuery.RemoveAll(x => !x.Propertyes.Any());

            return this;
        }

        public BaseFilteredQueryModel ToFilter() => this;
    }
}
