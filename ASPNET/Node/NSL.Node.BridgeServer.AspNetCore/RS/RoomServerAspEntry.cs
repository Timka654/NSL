using NSL.BuilderExtensions.WebSocketsServer;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerAspEntry : RoomServerBaseEntry
    {
        private readonly IEndpointRouteBuilder builder;
        private readonly string pattern;
        private readonly Func<HttpContext, Task<bool>> requestHandle;
        private readonly Action<IEndpointConventionBuilder> actionConventionBuilder;

        public RoomServerAspEntry(
            NodeBridgeServerEntry entry, 
            IEndpointRouteBuilder builder, 
            string pattern,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConventionBuilder = null, 
            string logPrefix = null) : base(entry, logPrefix)
        {
            this.builder = builder;
            this.pattern = pattern;
            this.requestHandle = requestHandle;
            this.actionConventionBuilder = actionConventionBuilder;

            var server = Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .AspWithOptions<NetworkClient, NetworkOptions>())
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
