﻿using Cipher.AES;
using Cipher.RSA;
using ConfigurationEngine;
using ConfigurationEngine.Providers.IConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocketCore.Extensions.Buffer;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketPhantom.AspNetCore.Network;
using SocketPhantom.Cipher;
using SocketServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utils.Helper.Network;

namespace SocketPhantom.AspNetCore
{
    public class PhantomHubsManager
    {
        private ConcurrentDictionary<Type, BasePhantomHub> hubs = new ConcurrentDictionary<Type, BasePhantomHub>();
        private ConcurrentDictionary<string, BasePhantomHub> hubPath = new ConcurrentDictionary<string, BasePhantomHub>();
        private readonly IServiceProvider baseProvider;

        private IConfiguration configuration => baseProvider.GetRequiredService<IConfiguration>();

        private PhantomNetworkServer server;
        private PhantomCipherProvider cipher;

        public PhantomHubsManager(IServiceProvider baseProvider) : this(baseProvider, new NonePhantomCipherProvider())
        {
        }

        public PhantomHubsManager(IServiceProvider baseProvider, PhantomCipherProvider cipher)
        {
            this.baseProvider = baseProvider;
            this.cipher = cipher;

            server = new PhantomNetworkServer();
            server.Load(this, configuration, cipher);
        }

        public void RegisterHub<TBase, THub>(IEndpointRouteBuilder endpoints, string url, PhantomHubOptions options)
            where THub : PhantomHub, TBase
        {
            var authAttribute = typeof(THub).GetCustomAttribute<AuthorizeAttribute>();

            var hub = new BasePhantomHub<THub>(baseProvider, authAttribute != null, options);

            hubPath.TryAdd(url, hub);

            hubs.TryAdd(typeof(TBase), hub);

            var action = endpoints.MapGet(url.TrimStart('/'), async context =>
            {
                context.Request.Query.TryGetValue("session", out var session);

                session = hub.GetSession(context, session);

                if (string.IsNullOrWhiteSpace(session))
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                context.Response.StatusCode = 200;

                await context.Response.WriteAsJsonAsync(new { session = session.First(), path = url, url = $"sp://{context.Request.Host.Host}:{server.GetOptions().Port}" });
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
