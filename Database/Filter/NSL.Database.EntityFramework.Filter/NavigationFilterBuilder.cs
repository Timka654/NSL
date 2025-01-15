using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NSL.Database.EntityFramework.Filter.Enums;
using NSL.Database.EntityFramework.Filter.Models;

namespace NSL.Database.EntityFramework.Filter
{
    [Obsolete("Use EntityFilterBuilder.")]
    public class NavigationFilterBuilder : EntityFilterBuilder
    {

    }

    public class EntityFilterBuilder : EntityFilterQueryModel
    {
        private EntityFilterBlockModel currentBlock = null;
        private EntityFilterOrderBlockModel currentOrderBlock = null;

        protected EntityFilterBuilder()
        {
            FilterQuery = new List<EntityFilterBlockModel>();
            OrderQuery = new List<EntityFilterOrderBlockModel>();
        }

        public static EntityFilterBuilder Create() => new EntityFilterBuilder();

        public EntityFilterBuilder AddPageOffset(int offset, int count)
        {
            Offset = offset;
            Count = count;
            return this;
        }

        public EntityFilterBuilder SetOffsetFromPage(int page, int pageSize)
        {
            return AddPageOffset((int)Math.Ceiling((page - 1.0) * pageSize), pageSize);
        }

        public EntityFilterBuilder AddFilter(string propertyPath, CompareType compareType, object value)
        {
            if (currentBlock == null)
                CreateFilterBlock();

            currentBlock.AddFilter(propertyPath, compareType, value);

            return this;
        }

        public EntityFilterBuilder AddFilter<T>(string propertyPath, CompareType compareType, T value) => AddFilter(propertyPath, compareType, (object)value);

        public EntityFilterBuilder CreateFilterBlock()
        {
            currentBlock = new EntityFilterBlockModel();
            FilterQuery.Add(currentBlock);
            return this;
        }

        public EntityFilterBuilder CreateFilterBlock(Action<EntityFilterBlockModel> buildBlock)
        {
            var cb = new EntityFilterBlockModel();
            buildBlock(cb);
            FilterQuery.Add(cb);
            return this;
        }


        public EntityFilterBuilder AddOrder(string propertyPath, bool asc = true)
        {
            if (currentOrderBlock == null)
                CreateOrderBlock();
            currentOrderBlock.Properties.Add(new FilterPropertyOrderViewModel() { PropertyPath = propertyPath, ASC = asc });
            return this;
        }

        public EntityFilterBuilder CreateOrderBlock()
        {
            currentOrderBlock = new EntityFilterOrderBlockModel();
            OrderQuery.Add(currentOrderBlock);
            return this;
        }

        public EntityFilterBuilder CreateOrderBlock(Action<EntityFilterOrderBlockModel> buildBlock)
        {
            var cb = new EntityFilterOrderBlockModel();
            buildBlock(cb);
            OrderQuery.Add(cb);
            return this;
        }

        public EntityFilterBuilder SetOffset(int offset)
        {
            Offset = offset;

            return this;
        }

        public EntityFilterBuilder SetCount(int count)
        {
            Count = count;

            return this;
        }

        public static EntityFilterQueryModel CreatePageOffsetFilter(int page, int pageSize)
        {
            return CreateOffsetFilter<EntityFilterQueryModel>((int)Math.Ceiling((page - 1.0) * pageSize), pageSize);
        }

        public static T CreateOffsetFilter<T>(int offset, int pageSize)
            where T : EntityFilterQueryModel, new()
        {
            return new T() { Count = pageSize, Offset = offset };
        }

        public static T CreatePageOffsetFilter<T>(int page, int pageSize)
            where T : EntityFilterQueryModel, new()
        {
            return CreateOffsetFilter<T>((int)Math.Ceiling((page - 1.0) * pageSize), pageSize);
        }

        public string ToJson() => JsonSerializer.Serialize(this);

        public EntityFilterBuilder ClearEmptyFilter()
        {
            FilterQuery.RemoveAll(x => !x.Properties.Any());

            return this;
        }

        public EntityFilterQueryModel ToFilter() => this;
    }
}
