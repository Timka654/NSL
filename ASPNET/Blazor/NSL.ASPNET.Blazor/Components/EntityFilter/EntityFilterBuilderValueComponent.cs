using System;

namespace NSL.ASPNET.Blazor.Components.EntityFilter
{
    public class EntityFilterBuilderValueComponent(Type componentType, Type componentData)
    {
        public Type ComponentType => componentType;

        public Type ComponentData { get; } = componentData;
    }
}
