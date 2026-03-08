using System.Collections.Generic;

namespace NSL.ASPNET.Blazor.Components.EntityFilter
{
    public interface IEntityFilterBuilderComponentData
    {

        void Initialize(EntityFilterBuilderBlockDataModel data, string fieldName, NSL.Entity.PathGenerator.Shared.FilterInfo field, EntityFilterBuilderBlockComponent component);

        Dictionary<string, object> GetParameters();
    }
}
