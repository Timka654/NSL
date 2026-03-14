using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Components
{
    public partial class BaseNSLTabButtonComponent : ComponentBase, IDisposable
    {
        [Parameter] public string? Name { get; set; }

        [CascadingParameter] protected BaseNSLTabsComponent TabsComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {

            if (TabsComponent != null)
            {
                TabsComponent.RegisterButton(this);

                TabsComponent.tabButtonChanged += TabsComponent_OnTabNameChanged;
            }
        }
        private async void TabsComponent_OnTabNameChanged(string obj)
        {
            if (Equals(Name, obj) && obj != default)
                await Select();

            StateHasChanged();
        }

        [Parameter] public EventCallback OnSelect { get; set; }

        private async Task Select()
        {
            await OnSelect.InvokeAsync();

            TabsComponent?.ShowTab(this, true);
        }

        public void Dispose()
        {
            if (TabsComponent != null)
                TabsComponent.tabButtonChanged -= TabsComponent_OnTabNameChanged;
        }
    }
}
