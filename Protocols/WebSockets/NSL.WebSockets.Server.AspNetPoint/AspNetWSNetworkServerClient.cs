using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSL.SocketServer.Utils;
using System;

namespace NSL.WebSockets.Server.AspNetPoint
{
    public class AspNetWSNetworkServerClient : IServerNetworkClient
    {
        public HttpContext HttpContext { get; private set; }

        internal void SetContext(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public IServiceProvider GetRequestServices()
            => HttpContext.RequestServices;

        public TService GetRequiredService<TService>()
            => GetRequestServices().GetRequiredService<TService>();

        public IConfiguration GetConfiguration()
            => GetRequiredService<IConfiguration>();

        public AsyncServiceScope CreateAsyncScope()
            => GetRequestServices().CreateAsyncScope();
    }
}
