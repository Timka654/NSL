using Microsoft.AspNetCore.Http;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Net;

namespace NSL.WebSockets.Server.AspNetPoint
{
    public class AspNetWSServerClient<T> : WSServerClient<T>
        where T : IServerNetworkClient, new()
    {
        private readonly HttpContext context;

        public AspNetWSServerClient(HttpContext context, ServerOptions<T> options) : base()
        {
            this.context = context;
            Initialize(options);
        }

        public override async void RunPacketReceiver()
        {
            try
            {
                sclient = await context.WebSockets.AcceptWebSocketAsync((string)null);

                RunReceive();

            }
            catch (Exception)
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
