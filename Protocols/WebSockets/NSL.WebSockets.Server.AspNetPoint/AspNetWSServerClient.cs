﻿using Microsoft.AspNetCore.Http;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NSL.WebSockets.Server.AspNetPoint
{
    public class AspNetWSServerClient<T> : WSServerClient<T>
        where T : AspNetWSNetworkServerClient, new()
    {
        private new readonly HttpContext context;

        public AspNetWSServerClient(HttpContext context, ServerOptions<T> options) : base()
        {
            this.context = context;
            Initialize(options);

            var client = (T)GetUserData();

            client.SetContext(context);
        }

        public override async Task RunPacketReceiver()
        {
            try
            {
                sclient = await context.WebSockets.AcceptWebSocketAsync();

                await base.ReceiveLoop();

            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        public override IPEndPoint GetRemotePoint()
        {
            if (context.Connection.RemoteIpAddress == default)
                return default;

            return new IPEndPoint(context.Connection.RemoteIpAddress, context.Connection.RemotePort);
        }
    }
}