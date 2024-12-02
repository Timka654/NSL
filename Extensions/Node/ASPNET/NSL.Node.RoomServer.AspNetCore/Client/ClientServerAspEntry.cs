using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.Node.RoomServer.Client;
using System.Threading.Tasks;
using System;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Node.RoomServer.Client.Data;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.WebSockets.Server;
using NSL.WebSockets.Server.AspNetPoint;
using System.Net;
using NSL.Node.BridgeServer.Shared;

namespace NSL.Node.RoomServer.AspNetCore.Client
{
    public class ClientServerAspEntry : ClientServerBaseEntry
    {
        private readonly IEndpointRouteBuilder builder;
        private readonly string pattern;
        private readonly NodeNetworkHandles<TransportNetworkClient> handles;
        private readonly Func<HttpContext, Task<bool>> requestHandle;
        private readonly Action<IEndpointConventionBuilder> actionConventionBuilder;

        private AspNetWebSocketsServer<TransportNetworkClient>.AcceptDelegate? acceptDelegate;

        public ClientServerAspEntry(
            NodeRoomServerEntry entry,
            IEndpointRouteBuilder builder,
            string pattern,
            NodeNetworkHandles<TransportNetworkClient> handles,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConventionBuilder = null,
            string logPrefix = null) : base(entry, logPrefix)
        {
            this.builder = builder;
            this.pattern = pattern;
            this.handles = handles;
            this.requestHandle = requestHandle;
            this.actionConventionBuilder = actionConventionBuilder;

            var convBuilder = builder.Map(pattern, async context =>
            {
                if (requestHandle != null)
                    if (!await requestHandle(context))
                        return;
                if (acceptDelegate != null)
                    await acceptDelegate(context);
                else
                    context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
            });

            if (actionConventionBuilder != null)
                actionConventionBuilder(convBuilder);
        }

        public override void Run()
        {
            if (Listener == null)
            {
                var server = handles.Fill(Fill(WebSocketsServerEndPointBuilder.Create()
                    .WithClientProcessor<TransportNetworkClient>()
                    .AspWithOptions<TransportNetworkClient, WSServerOptions<TransportNetworkClient>>()))
                    .BuildWithoutRoute();

                Listener = server;

                acceptDelegate = server.GetAcceptDelegate();
            }
        }
    }
}
