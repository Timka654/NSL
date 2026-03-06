using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System;
using NSL.Node.RoomServer.AspNetCore.Client;
using Microsoft.Extensions.DependencyInjection;
using NSL.Logger.AspNet;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;

namespace NSL.Node.RoomServer.AspNetCore
{
    public static class ASPNETStartupExtensions
    {
        public static IServiceCollection AddNodeRoomServer(this IServiceCollection serviceProvider)
            => serviceProvider.AddNodeRoomServer<NodeRoomServerEntryBuilder>();

        public static IServiceCollection AddNodeRoomServer<TBuilder>(this IServiceCollection serviceProvider)
            where TBuilder : NodeRoomServerEntryBuilder
        {
            serviceProvider.AddSingleton<TBuilder>();

            return serviceProvider;
        }

        public static IEndpointRouteBuilder RunNodeRoomServer(this IEndpointRouteBuilder serviceProvider, Action<NodeRoomServerEntryBuilder> configuration)
            => serviceProvider.RunNodeRoomServer<NodeRoomServerEntryBuilder>(configuration);

        public static IEndpointRouteBuilder RunNodeRoomServer<TBuilder>(this IEndpointRouteBuilder serviceProvider, Action<TBuilder> configuration)
            where TBuilder : NodeRoomServerEntryBuilder
        {
            var builder = serviceProvider.ServiceProvider.GetRequiredService<TBuilder>();

            configuration(builder);

            builder.Run();

            return serviceProvider;
        }

        public static NodeRoomServerEntryBuilder WithClientServerAspBinding(
            this NodeRoomServerEntryBuilder builder
            , IEndpointRouteBuilder aspBuilder
            , string pattern
            , NodeNetworkHandles<TransportNetworkClient> handles
            , Func<HttpContext, Task<bool>> requestHandle = null
            , Action<IEndpointConventionBuilder> actionConventionBuilder = null
            , string logPrefix = null)
            => builder.WithClientServerListener(new ClientServerAspEntry(builder.Entry, aspBuilder, pattern, handles, requestHandle, actionConventionBuilder, logPrefix));

        public static NodeRoomServerEntryBuilder WithAspLogger(this NodeRoomServerEntryBuilder builder, Microsoft.Extensions.Logging.ILogger logger)
            => builder.WithLogger(new ILoggerWrapper(logger));

    }
}