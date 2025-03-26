using System;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class EntityFilterResultModel<TData>
    {
        public TData[] Data { get; set; }

        public long Count { get; set; }
    }

    [Obsolete("Use \"EntityFilterResultModel\"", true)]
    public class FilterResultModel<TData> : EntityFilterResultModel<TData> { }

}
