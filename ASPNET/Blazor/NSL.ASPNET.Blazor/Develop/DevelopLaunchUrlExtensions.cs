using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Develop
{
    public static class DevelopLaunchUrlExtensions
    {
        public static void HandleDevelopmentLaunchUrl(this NavigationManager navigationManager
            , Func<HttpClient> clientGetter
            , string url = "api/develop/setLaunchUrl"
            , bool onlyDebuggerAttached = true)
        {
            navigationManager.LocationChanged += async (s, e) =>
            {
                if (!Debugger.IsAttached && onlyDebuggerAttached)
                {
                    Console.WriteLine($"Debugger detached - skip to send launchUrl data");
                    return;
                }

                try
                {
                    var client = clientGetter();

                    if (client != null)
                        await client.PostAsJsonAsync(url, new Uri(e.Location).PathAndQuery);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Develop] Error on send launch url - {ex.ToString()}");
                }

            };
        }
    }
}
