using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Components.Tab
{
    public abstract class BaseNSLTabButtonComponent : ComponentBase, IDisposable
    {
        [Parameter] public string? Name { get; set; }

        [CascadingParameter] protected BaseNSLTabsComponent TabsComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            TabsComponent?.RegisterButton(this);
        }

        [Parameter] public EventCallback OnSelect { get; set; }

        protected async Task Select()
        {
            await TabsComponent?.ShowTabAsync(this);
        }

        public void Dispose()
        {
            TabsComponent?.UnRegisterButton(this);
        }
    }
}
