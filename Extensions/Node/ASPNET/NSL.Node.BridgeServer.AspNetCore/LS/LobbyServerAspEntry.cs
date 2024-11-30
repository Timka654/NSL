using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.BuilderExtensions.WebSocketsServer;
using System.Threading.Tasks;
using System;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;
using NSL.Node.BridgeServer.Shared;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerAspEntry : LobbyServerBaseEntry
    {
        private readonly IEndpointRouteBuilder builder;
        private readonly string pattern;
        private readonly NodeNetworkHandles<NetworkClient> handles;
        private readonly Func<HttpContext, Task<bool>> requestHandle;
        private readonly Action<IEndpointConventionBuilder> actionConventionBuilder;

        public LobbyServerAspEntry(
            NodeBridgeServerEntry entry,
            IEndpointRouteBuilder builder,
            string pattern,
            NodeNetworkHandles<NetworkClient> handles,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConventionBuilder = null,
            string logPrefix = null) : base(entry, logPrefix)
        {
            this.builder = builder;
            this.pattern = pattern;
            this.handles = handles;
            this.requestHandle = requestHandle;
            this.actionConventionBuilder = actionConventionBuilder;

            var server = handles.Fill(Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .AspWithOptions<NetworkClient, NetworkOptions>()))
                .BuildWithoutRoute();

            var acceptDelegate = server.GetAcceptDelegate();

            var convBuilder = builder.Map(pattern, async context =>
            {
                if (requestHandle != null)
                    if (!await requestHandle(context))
                        return;

                await acceptDelegate(context);
            });

            if (actionConventionBuilder != null)
                actionConventionBuilder(convBuilder);


            Listener = server;
        }

        public override void Run()
        {
        }
    }
}
