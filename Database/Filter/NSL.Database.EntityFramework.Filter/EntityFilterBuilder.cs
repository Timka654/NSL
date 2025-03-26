using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NSL.Database.EntityFramework.Filter.Models;

namespace NSL.Database.EntityFramework.Filter
{
    public abstract class EntityFilterBuilder
    {
        #region Static

        public static EntityFilterTypedBuilder<EntityFilterQueryModel> Create()
            => Create<EntityFilterQueryModel>(out _);

        public static EntityFilterTypedBuilder<TQuery> Create<TQuery>()
            where TQuery : EntityFilterQueryModel, new()
            => Create<TQuery>(out _);

        public static EntityFilterTypedBuilder<EntityFilterQueryModel> Create(out EntityFilterQueryModel filter)
            => Create<EntityFilterQueryModel>(out filter);

        public static EntityFilterTypedBuilder<TQuery> Create<TQuery>(out TQuery filter)
            where TQuery : EntityFilterQueryModel, new()
            => new EntityFilterTypedBuilder<TQuery>(filter = new TQuery());

        public static EntityFilterQueryModel CreatePageOffsetFilter(int page, int pageSize)
        {
            return CreateOffsetFilter<EntityFilterQueryModel>((int)Math.Ceiling((page - 1.0) * pageSize), pageSize);
        }

        public static TQuery CreatePageOffsetFilter<TQuery>(int page, int pageSize)
            where TQuery : EntityFilterQueryModel, new()
        {
            return CreateOffsetFilter<TQuery>((int)Math.Ceiling((page - 1.0) * pageSize), pageSize);
        }

        public static T CreateOffsetFilter<T>(int offset, int pageSize)
            where T : EntityFilterQueryModel, new()
        {
            return new T() { Count = pageSize, Offset = offset };
        }
        #endregion


        public EntityFilterBuilder()
        {
        }

        public abstract int Offset { get; set; }

        public abstract int Count { get; set; }

        public abstract List<EntityFilterBlockModel>? FilterQuery { get; set; }

        public abstract List<EntityFilterOrderBlockModel>? OrderQuery { get; set; }
    }

    public class EntityFilterTypedBuilder<TQuery> : EntityFilterBuilder
        where TQuery : EntityFilterQueryModel
    {
        TQuery filter;

        public override int Offset { get => filter.Offset; set => filter.Offset = value; }

        public override int Count { get => filter.Count; set => filter.Count = value; }

        public override List<EntityFilterBlockModel> FilterQuery { get => filter.FilterQuery; set => filter.FilterQuery = value; }

        public override List<EntityFilterOrderBlockModel> OrderQuery { get => filter.OrderQuery; set => filter.OrderQuery = value; }

        public EntityFilterTypedBuilder(TQuery _filter)
        {
            _filter.FilterQuery = new List<EntityFilterBlockModel>();
            _filter.OrderQuery = new List<EntityFilterOrderBlockModel>();

            this.filter = _filter;
        }

        public string ToJson() => JsonSerializer.Serialize(filter);

        public TQuery GetFilter() => filter;
    }
}
