using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.SocketPhantom.AspNetCore.Network;
using NSL.SocketPhantom.AspNetCore.Network.Packets;
using NSL.SocketPhantom.Cipher;
using NSL.SocketPhantom.Enums;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace NSL.SocketPhantom.AspNetCore
{
    public class PhantomHubsManager
    {
        private ConcurrentDictionary<Type, BasePhantomHub> hubs = new ConcurrentDictionary<Type, BasePhantomHub>();
        private ConcurrentDictionary<string, BasePhantomHub> hubPath = new ConcurrentDictionary<string, BasePhantomHub>();
        private readonly IServiceProvider baseProvider;
        private readonly PhantomCipherProvider cipher;

        public PhantomHubsManager(IServiceProvider baseProvider) : this(baseProvider, new NonePhantomCipherProvider())
        {
        }

        public PhantomHubsManager(IServiceProvider baseProvider, PhantomCipherProvider cipher)
        {
            this.baseProvider = baseProvider;
            this.cipher = cipher;
        }

        public void RegisterHub<TBase, THub>(IEndpointRouteBuilder endpoints, string url, PhantomHubOptions options)
            where THub : PhantomHub, TBase
        {
            var authAttribute = typeof(THub).GetCustomAttribute<AuthorizeAttribute>();

            var hub = new BasePhantomHub<THub>(baseProvider, authAttribute != null, options);

            hubPath.TryAdd(url, hub);

            hubs.TryAdd(typeof(TBase), hub);

            var action = endpoints.MapWebSocketsPoint<PhantomHubClientProxy>(url, builder =>
            {
                cipher.SetProvider(builder.GetCoreOptions());

                builder.AddPacket(PacketEnum.SignIn, new SessionPacket(this));
                builder.AddPacket(PacketEnum.Invoke, new InvokePacket());
            }, async context =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Request.Query.TryGetValue("session", out var session);

                    session = hub.GetSession(context, session);

                    if (string.IsNullOrWhiteSpace(session))
                    {
                        context.Response.StatusCode = 400;
                    }
                    else
                    {
                        context.Response.StatusCode = 200;

                        await context.Response.WriteAsJsonAsync(new
                        {
                            session = session.First(),
                            path = url
                        });
                    }
                    //context.Connection.RequestClose();

                    return false;
                }

                return true;
            });

            if (hub.RequiredAuth)
                action.WithMetadata(authAttribute);
        }

        public BasePhantomHub GetHub(string relativeUrl)
        {
            hubPath.TryGetValue(relativeUrl, out var hub);

            return hub;
        }

        public BasePhantomHub<THub> GetHub<THub>()
            where THub : PhantomHub
        {
            hubs.TryGetValue(typeof(THub), out var hub);

            return hub as BasePhantomHub<THub>;
        }

        public void RegisterHub<THub>(IEndpointRouteBuilder endpoints, string url, PhantomHubOptions options)
            where THub : PhantomHub
        {
            RegisterHub<THub, THub>(endpoints, url, options);
        }

        public bool ProcessClient(PhantomHubClientProxy client, string relativeUrl, out BasePhantomHub hub)
        {
            hub = GetHub(relativeUrl);

            if (hub == null)
                return false;

            return hub.LoadClient(client);
        }
    }
}
