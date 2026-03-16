using Microsoft.AspNetCore.Components;
using NSL.ASPNET.Blazor.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Components
{
    public partial class BaseNSLTabsComponent : ComponentBase, IDisposable
    {
        [Inject] NSLTabsService TabsService { get; set; }

        public BaseNSLTabButtonComponent? SelectedTab { get; protected set; }

        /// <summary>
        /// name of component
        /// default: tab
        /// </summary>
        [Parameter]
        public string? Name { get; set; } = "tab";

        [Parameter] public string? TabName { get; set; }

        [Parameter] public bool UrlNavigation { get; set; } = true;

        [Parameter] public bool NoMargins { get; set; } = false;


        public event Action<string?> TabNameChanged = name => { };


        public event Action<string?> TabButtonChanged = name => { };

        protected override async Task OnInitializedAsync()
        {
            if (UrlNavigation)
            {
                var tabName = TabsService.RegisterNSLTab(this);

                if (tabName.tabName != default && !tabName.exists)
                {
                    ShowTab(tabName.tabName);
                }
            }
        }
        public void ShowTab(BaseNSLTabButtonComponent selectedTab, bool buttonSrc = false)
        {
            if (selectedTab == SelectedTab)
                return;

            this.SelectedTab = selectedTab;

            TabName = SelectedTab?.Name;

            if (UrlNavigation)
                TabsService.UpdateNSLTabUrl(this);

            TabNameChanged(TabName);

            if (!buttonSrc) TabButtonChanged(TabName);

            StateHasChanged();
        }

        public void ShowTab(string name)
        {
            TabName = name;
            TabNameChanged(name);
            TabButtonChanged(name);

            StateHasChanged();
        }

        public bool Disposed { get; protected set; }

        public virtual void Dispose()
        {
            Disposed = true;

            if (UrlNavigation)
                TabsService.UnregisterNSLTab(this);
        }

        int frst = 0;

        public virtual void RegisterButton(BaseNSLTabButtonComponent newTab)
        {
            if (TabName == default)
            {
                if (Interlocked.Increment(ref frst) > 1)
                {

                    return;
                }

                if (newTab != default)
                {
                    ShowTab(newTab);
                }
            }
        }
    }
}
