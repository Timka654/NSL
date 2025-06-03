using System;

namespace NSL.ASPNET.Blazor.Context
{
    public interface IComponentUpdatableContext : IComponentContext
    {
        event Action OnUpdate;

        void Update();
    }
}
