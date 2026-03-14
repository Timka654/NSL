using Microsoft.AspNetCore.Components;
using NSL.ASPNET.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NSL.ASPNET.Blazor.Services
{
    public class NSLTabsService
    {
        private readonly NavigationManager navigationManager;

        public NSLTabsService(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;

            navigationManager.LocationChanged += NavigationManager_LocationChanged;
        }

        private void NavigationManager_LocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            var parameters = HttpUtility.ParseQueryString(new Uri(navigationManager.Uri).Query);


            foreach (var item in tabs)
            {
                var tn = parameters.GetValues(item.Key)?.FirstOrDefault();

                if (tn == default) continue;

                var iv = item.Value;

                if (iv == default) continue;

                if (iv.TabName != tn)
                    iv.ShowTab(tn);
            }
            //UpdateNSLTabUrl();
        }

        internal (string? tabName, bool exists) RegisterNSLTab(BaseNSLTabsComponent component)
        {
            //if (component.Name == default)
            //    return default;

            //tabs[component.Name] = component;

            var parameters = HttpUtility.ParseQueryString(new Uri(navigationManager.Uri).Query);
            var vals = parameters.GetValues(component.Name);

            return (vals?.FirstOrDefault(), component.Name != default && tabs.TryGetValue(component.Name, out var o) && o != default);
        }

        internal string? GetNSLTabName(BaseNSLTabsComponent component)
        {
            var parameters = HttpUtility.ParseQueryString(new Uri(navigationManager.Uri).Query);
            var vals = parameters.GetValues(component.Name);

            return vals?.FirstOrDefault();
        }

        internal async void UnregisterNSLTab(BaseNSLTabsComponent component)
        {
            if (component.Name == default)
                return;
            if (tabs.Remove(component.Name, out var o) && o != component && o != null && !o.Disposed)
                RegisterNSLTab(o);
            else if (o != default)
            {
                tabs[component.Name] = null;
                await Task.Delay(100);
                UpdateNSLTabUrl();
            }
        }

        internal void UpdateNSLTabComponent(BaseNSLTabsComponent component)
        {
            if (component.Name == default)
                return;

            tabs[component.Name] = component;
        }

        internal void UpdateNSLTabUrl(BaseNSLTabsComponent component)
        {
            if (component.Name == default)
                return;

            tabs[component.Name] = component;

            UpdateNSLTabUrl();
        }

        private void UpdateNSLTabUrl()
        {
            Dictionary<string, object?> vals = new();

            foreach (var tab in tabs)
            {
                vals[tab.Key] = tab.Value?.TabName;
            }

            var newUrl = navigationManager.GetUriWithQueryParameters(vals);

            if (newUrl != navigationManager.Uri)
                navigationManager.NavigateTo(navigationManager.GetUriWithQueryParameters(vals), replace: true);
        }

        private Dictionary<string, BaseNSLTabsComponent?> tabs = new();
    }
}
