using Microsoft.AspNetCore.Components;
using NSL.ASPNET.Blazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Components.Tab
{
    public abstract class BaseNSLTabsComponent : ComponentBase, IDisposable
    {
        [Inject] protected NSLTabsService TabsService { get; set; }

        /// <summary>
        /// name of component
        /// default: tab
        /// </summary>
        [Parameter]
        public string? Name { get; set; } = "tab";

        [Parameter] public string? TabName { get; set; }

        [Parameter] public bool UrlNavigation { get; set; } = true;

        public bool Disposed { get; protected set; }

        protected override async Task OnInitializedAsync()
        {
            if (UrlNavigation)
            {
                var tabName = TabsService.RegisterNSLTab(this);

                if (tabName.tabName != default && !tabName.exists)
                {
                    await ShowTabAsync(tabName.tabName);
                }
            }
        }

        public virtual void Dispose()
        {
            Disposed = true;

            if (UrlNavigation)
                TabsService.UnregisterNSLTab(this);
        }


        public event Action<BaseNSLTabButtonComponent?> TabChanged = name => { };

        protected void OnTabChanged(BaseNSLTabButtonComponent? selectedTab)
        {
            TabChanged(selectedTab);
        }

        public virtual void UnRegisterButton(BaseNSLTabButtonComponent newTab)
        {
        }

        public virtual void RegisterButton(BaseNSLTabButtonComponent newTab)
        {
        }

        internal virtual async Task ShowTabAsync(BaseNSLTabButtonComponent selectedTab)
        {
            OnTabChanged(selectedTab);
        }

        public virtual async Task ShowTabAsync(string name)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class BaseNSLTabsComponent<TTabButton> : BaseNSLTabsComponent
        where TTabButton : BaseNSLTabButtonComponent
    {
        public TTabButton? SelectedTab { get; protected set; }

        protected List<TTabButton> TabButtons { get; } = new List<TTabButton>();

        public new event Action<TTabButton?> TabChanged = name => { };

        internal override async Task ShowTabAsync(BaseNSLTabButtonComponent selectedTab)
        {
            if (!TabButtons.Contains(selectedTab))
                throw new Exception("Invalid tab type");

            await ShowTabAsync((TTabButton)selectedTab);

        }

        public virtual async Task ShowTabAsync(TTabButton selectedTab)
        {
            if (selectedTab == SelectedTab)
                return;

            if (selectedTab?.OnSelect.HasDelegate == true)
                await selectedTab.OnSelect.InvokeAsync();

            this.SelectedTab = selectedTab;

            TabName = SelectedTab?.Name;

            if (UrlNavigation)
                TabsService.UpdateNSLTabUrl(this);

            base.OnTabChanged(SelectedTab);

            StateHasChanged();
        }

        public override async Task ShowTabAsync(string name)
        {
            var tab = TabButtons.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (tab != null)
                await ShowTabAsync(tab);
            else
                throw new Exception($"Tab with name '{name}' not found.");
        }

        public override void UnRegisterButton(BaseNSLTabButtonComponent newTab)
        {
            if (newTab is TTabButton typedTab)
            {
                TabButtons.Remove(typedTab);
            }
            else
                throw new Exception("Invalid tab type");
        }

        public override void RegisterButton(BaseNSLTabButtonComponent newTab)
        {
            if (newTab is TTabButton typedTab)
            {
                TabButtons.Add(typedTab);
            }
            else
                throw new Exception("Invalid tab type");
        }
    }
}
