using System;
using System.Collections.Generic;

namespace NSL.ASPNET.Blazor.Components.EntityFilter
{
    public interface IEntityFilterBuilderFragmentComponent
    {
        void Update();
        void Remove(EntityFilterBuilderBlockDataModel block);
        bool IsFirst(EntityFilterBuilderBlockDataModel block);

        IReadOnlyDictionary<Type, IReadOnlyDictionary<string, NSL.Entity.PathGenerator.Shared.FilterInfo>> TypeLibrary
        {
            get;
        }
    }
}
