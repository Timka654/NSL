using NSL.Database.EntityFramework.Filter.Enums;
using NSL.Database.EntityFramework.Filter.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Database.EntityFramework.Filter
{
    public static class EntityFilterQueryExtensions
    {
        public static TBuilder AddPageOffset<TBuilder>(this TBuilder builder, int offset, int count)
            where TBuilder : EntityFilterBuilder
        {
            builder.Offset = offset;
            builder.Count = count;
            return builder;
        }

        public static TBuilder SetOffsetFromPage<TBuilder>(this TBuilder builder, int page, int pageSize)
            where TBuilder : EntityFilterBuilder
        {
            return builder.AddPageOffset((int)Math.Ceiling((page - 1.0) * pageSize), pageSize);
        }


        public static TBuilder AddFilterProperty<TBuilder,TValue>(this TBuilder builder, string propertyPath, CompareType compareType, TValue value)
            where TBuilder : EntityFilterBuilder
            => builder.AddFilterProperty(propertyPath, compareType, (object)value);

        public static TBuilder AddFilterProperty<TBuilder>(this TBuilder builder, string propertyPath, CompareType compareType, object value)
            where TBuilder : EntityFilterBuilder
            => builder.CreateFilterBlock(b => b.AddProperty(propertyPath, compareType, value));

        public static TBuilder CreateFilterBlock<TBuilder>(this TBuilder builder, Action<EntityFilterBlockModel> buildBlock)
            where TBuilder : EntityFilterBuilder
        {
            if (builder.FilterQuery == null)
                builder.FilterQuery = new List<EntityFilterBlockModel>() { };

            var cb = new EntityFilterBlockModel();

            buildBlock(cb);

            builder.FilterQuery.Add(cb);

            return builder;
        }

        public static TBuilder AddOrderProperty<TBuilder>(this TBuilder builder, string propertyPath, bool asc = true)
            where TBuilder : EntityFilterBuilder
            => builder.CreateOrderBlock(b => b.AddProperty(propertyPath, asc));

        public static TBuilder CreateOrderBlock<TBuilder>(this TBuilder builder, Action<EntityFilterOrderBlockModel> buildBlock)
            where TBuilder : EntityFilterBuilder
        {
            if (builder.OrderQuery == null)
                builder.OrderQuery = new List<EntityFilterOrderBlockModel>() { };

            var cb = new EntityFilterOrderBlockModel();

            buildBlock(cb);

            builder.OrderQuery.Add(cb);

            return builder;
        }

        public static TBuilder SetOffset<TBuilder>(this TBuilder builder, int offset)
            where TBuilder : EntityFilterBuilder
        {
            builder.Offset = offset;

            return builder;
        }

        public static TBuilder SetCount<TBuilder>(this TBuilder builder, int count)
            where TBuilder : EntityFilterBuilder
        {
            builder.Count = count;

            return builder;
        }

        public static TBuilder ClearEmptyFilter<TBuilder>(this TBuilder builder)
            where TBuilder : EntityFilterBuilder
        {
            builder.FilterQuery.RemoveAll(x => !x.Properties.Any());

            return builder;
        }
    }
}
