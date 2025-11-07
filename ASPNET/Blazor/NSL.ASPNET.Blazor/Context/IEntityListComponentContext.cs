using System.Collections.Generic;

namespace NSL.ASPNET.Blazor.Context
{
    public interface IEntityListComponentContext<TEntity> : IComponentContext
    {
        List<TEntity> Items { get; }
    }
}
