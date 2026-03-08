namespace NSL.ASPNET.Blazor.Components.EntityFilter
{
    public interface IEntityFilterBuilderComponentRegistry
    {
        EntityFilterBuilderValueComponent? GetValueComponent(EntityFilterBuilderBlockDataModel data);
    }
}
