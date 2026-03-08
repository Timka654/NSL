using System;

namespace NSL.ASPNET.Blazor.Components.EntityFilter
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EntityFilterBuilderTypeValueComponentAttribute(Type type, Type componentData) : Attribute
    {
        public Type Type { get; } = type;

        public Type ComponentData { get; } = componentData;
    }
}
