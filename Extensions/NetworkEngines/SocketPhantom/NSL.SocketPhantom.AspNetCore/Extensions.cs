using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocketPhantom;
using SocketPhantom.Cipher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.AspNetCore
{
    public static class Extensions
    {
        public static void AddPhantomProtocol(this IServiceCollection collection)
        {
            collection.AddSingleton(s => new PhantomHubsManager(s));
        }
        public static void AddPhantomProtocol(this IServiceCollection collection, PhantomCipherProvider cipher)
        {
            collection.AddSingleton(s => new PhantomHubsManager(s, cipher));
        }

        public static void MapPhantomHub<Hub>(this IEndpointRouteBuilder endpoints, string relativeUrl)
            where Hub : PhantomHub
        {
            MapPhantomHub<Hub>(endpoints, relativeUrl, new PhantomHubOptions() { DisconnectTimeOut = TimeSpan.FromSeconds(15) });
        }

        public static void MapPhantomHub<Hub>(this IEndpointRouteBuilder endpoints, string relativeUrl, PhantomHubOptions options)
            where Hub : PhantomHub
        {
            endpoints.ServiceProvider.GetRequiredService<PhantomHubsManager>().RegisterHub<Hub>(endpoints, relativeUrl, options);
        }

        public static Task SendCoreAsync(this IClientProxy proxy, string name, params object[] p) => proxy.SendCoreAsync(name, p);
    }
}
