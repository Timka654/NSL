﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketPhantom.AspNetCore.Network;
using NSL.SocketPhantom.Cipher;
using NSL.SocketPhantom.Enums;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

                builder.AddAsyncPacketHandle(PacketEnum.SignIn, SessionPacketHandle);
                builder.AddAsyncPacketHandle(PacketEnum.Invoke, InvokePacketHandle);
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

        public async Task SessionPacketHandle(PhantomHubClientProxy client, InputPacketBuffer data)
        {
            var path = data.ReadString();

            client.Session = data.ReadString();

            var sessionResultPacket = OutputPacketBuffer.Create(PacketEnum.SignInResult);

            if (ProcessClient(client, path, out var hub))
                sessionResultPacket.WriteByte(byte.MaxValue);
            else if (hub == null)
                sessionResultPacket.WriteByte((byte)SignStatusCodeEnum.ErrorPath);
            else
                sessionResultPacket.WriteByte((byte)SignStatusCodeEnum.ErrorSession);

            client.Send(sessionResultPacket);
        }

        public async Task InvokePacketHandle(PhantomHubClientProxy client, InputPacketBuffer data)
        {
            await client.Hub.Invoke(client, data);
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
