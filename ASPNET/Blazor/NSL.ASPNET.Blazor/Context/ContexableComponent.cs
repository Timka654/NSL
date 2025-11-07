using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Context
{
    public class ContexableComponent<TContext> : ComponentBase, IDisposable
        where TContext : IComponentContext
    {
        private TContext? context;

        [Parameter] public TContext? Context { get => context; set => ChangeContext(context, value); }

        public virtual void Dispose()
        {
            var context = Context;

            if (context != null && context is IUpdatableComponentContext ou)
                ou.OnUpdate -= Context_OnUpdate;
        }

        protected virtual void ChangeContext(TContext? oldContext, TContext? newContext)
        {
            if (newContext != null && newContext is IUpdatableComponentContext nu)
                nu.OnUpdate += Context_OnUpdate;

            context = newContext;

            if (oldContext != null && oldContext is IUpdatableComponentContext ou)
                ou.OnUpdate -= Context_OnUpdate;

        }

        protected void Context_OnUpdate()
        {
            StateHasChanged();
        }
    }
}
