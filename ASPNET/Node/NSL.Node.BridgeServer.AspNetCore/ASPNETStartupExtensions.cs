using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NSL.Logger.AspNet;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.RS;
using NSL.Node.BridgeServer.Shared;
using NSL.SocketCore.Utils;
using System;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer
{
    public static class ASPNETStartupExtensions
    {
        public static IServiceCollection AddNodeBridgeServer(this IServiceCollection serviceProvider)
            => serviceProvider.AddNodeBridgeServer<NodeBridgeServerEntryBuilder>();

        public static IServiceCollection AddNodeBridgeServer<TBuilder>(this IServiceCollection serviceProvider)
            where TBuilder : NodeBridgeServerEntryBuilder
        {
            serviceProvider.AddSingleton<TBuilder>();

            return serviceProvider;
        }

        public static IEndpointRouteBuilder RunNodeBridgeServer(this IEndpointRouteBuilder serviceProvider, Action<NodeBridgeServerEntryBuilder> configuration)
            => serviceProvider.RunNodeBridgeServer<NodeBridgeServerEntryBuilder>(configuration);

        public static IEndpointRouteBuilder RunNodeBridgeServer<TBuilder>(this IEndpointRouteBuilder serviceProvider, Action<TBuilder> configuration)
            where TBuilder : NodeBridgeServerEntryBuilder
        {
            var builder = serviceProvider.ServiceProvider.GetRequiredService<TBuilder>();

            configuration(builder);

            builder.Run();

            return serviceProvider;
        }

        public static NodeBridgeServerEntryBuilder WithLobbyServerAspBinding(
            this NodeBridgeServerEntryBuilder builder,
        IEndpointRouteBuilder aspBuilder,
            string pattern
            , NodeNetworkHandles<LobbyServerNetworkClient> handles
            , Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConventionBuilder = null,
            string logPrefix = null)
            => builder.WithLobbyServerListener(new LobbyServerAspEntry(builder.Entry, aspBuilder, pattern, handles, requestHandle, actionConventionBuilder, logPrefix));

        public static NodeBridgeServerEntryBuilder WithRoomServerAspBinding(
            this NodeBridgeServerEntryBuilder builder,
            IEndpointRouteBuilder aspBuilder,
            string pattern,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConventionBuilder = null,
            string logPrefix = null)
            => builder.WithRoomServerListener(new RoomServerAspEntry(builder.Entry, aspBuilder, pattern, requestHandle, actionConventionBuilder, logPrefix));


        public static NodeBridgeServerEntryBuilder WithAspLogger(
            this NodeBridgeServerEntryBuilder builder, Microsoft.Extensions.Logging.ILogger logger)
            => builder.WithLogger(new ILoggerWrapper(logger));
    }
}
