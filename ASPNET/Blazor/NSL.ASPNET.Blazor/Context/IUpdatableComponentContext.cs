using System;

namespace NSL.ASPNET.Blazor.Context
{
    public interface IUpdatableComponentContext : IComponentContext
    {
        event Action OnUpdate;

        void Update();
    }
}
