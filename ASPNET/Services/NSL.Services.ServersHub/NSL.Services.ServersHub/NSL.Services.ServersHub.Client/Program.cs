using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NSL.Services.ServersHub.Client;
using NSL.ASPNET.Identity;
using NSL.ASPNET.Identity.JWT;
using NSL.ASPNET.Identity.ClientIdentity;
using NSL.ASPNET.Identity.ClientIdentity.MessageHandlers;
using NSL.ASPNET.Identity.ClientIdentity.Providers;
using NSL.Services.ServersHub.Client.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
namespace NSL.Services.ServersHub.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<Routes>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddHttpClient("ServerAPI", (sp, client) => client.FillHttpClientIdentity(sp)
                .BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                })
                .AddHttpMessageIdentityHandler();

            builder.Services.AddBlazoredLocalStorageAsSingleton();

            builder.Services
                .AddIdentityPolicyProvider<IdentityPolicyProvider>()
                .AddHttpMessageIdentityHandler()
                .AddIdentityStateProvider<IdentityStateProvider>()
                .AddIdentityService<HubIdentityService>()
                .AddIdentityAuthorizationService<IdentityAuthorizationService>();

            //builder.Services.AddAuthorizationCore();
            //builder.Services.AddCascadingAuthenticationState();


            var app = builder.Build();

            await app.Services.LoadIdentityAsync();

            await app.RunAsync();
        }
    }
}
