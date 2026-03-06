using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.LocalBridge;
using NSL.Logger.AspNet;
using NSL.Node.BridgeLobbyClient.Models;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Client;
using System;

namespace NSL.Node.BridgeLobbyClient.AspNetCore
{
    public static class Extensions
    {
        /// <summary>
        /// Add <see cref="BridgeLobbyBaseNetwork"/> to service list
        /// </summary>
        /// <param name="services"></param>
        /// <param name="url"></param>
        /// <param name="serverIdentity"></param>
        /// <param name="identityKey"></param>
        /// <param name="onBuild"></param>
        /// <returns></returns>
        public static IServiceCollection AddNodeBridgeLobbyClient(
            this IServiceCollection services,
            string url,
            string serverIdentity,
            string identityKey,
            Action<IServiceProvider, BridgeLobbyNetworkHandlesConfigurationModel> onHandleConfiguration,
            Action<IServiceProvider, WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null
            )
            => services.AddSingleton(services => new BridgeLobbyNetwork(
                new Uri(url),
                serverIdentity,
                identityKey,
                (handles) => onHandleConfiguration(services, handles),
                builder =>
            {
                builder.SetLogger(new ILoggerWrapper(services.GetRequiredService<ILogger<BridgeLobbyNetwork>>()));

                if (onBuild != null) onBuild(services, builder);
            }));

        public static IServiceCollection AddNodeBridgeLobbyLocalBridgeClient<TServerClient>(
            this IServiceCollection services,
            string serverIdentity,
            string identityKey,
            Action<IServiceProvider, BridgeLobbyNetworkHandlesConfigurationModel> onHandleConfiguration,
            Action<IServiceProvider, WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null
            )
            where TServerClient : INetworkClient, new()
            => services.AddSingleton(services => new BridgeLobbyLocalBridgeNetwork<TServerClient>(
                serverIdentity,
                identityKey,
                (handles) => onHandleConfiguration(services, handles),
                builder =>
            {
                builder.SetLogger(new ILoggerWrapper(services.GetRequiredService<ILogger<BridgeLobbyLocalBridgeNetwork<TServerClient>>>()));

                if (onBuild != null) onBuild(services, builder);
            }));

        public static void RunNodeBridgeLobbyLocalBridgeClient<TServerClient>(this IEndpointRouteBuilder host, LocalBridgeClient<TServerClient, BridgeLobbyNetworkClient> serverClient)
            where TServerClient : INetworkClient, new()
            => GetNodeBridgeLobbyClient<BridgeLobbyLocalBridgeNetwork<TServerClient>>(host)
            .WithServerClient(serverClient)
            .Initialize();

        public static void RunNodeBridgeLobbyClient(this IEndpointRouteBuilder host)
            => RunNodeBridgeLobbyClient<BridgeLobbyNetwork>(host);

        public static void RunNodeBridgeLobbyClient<TNetwork>(this IEndpointRouteBuilder host)
            where TNetwork : BridgeLobbyBaseNetwork
            => GetNodeBridgeLobbyClient<TNetwork>(host).Initialize();

        private static TNetwork GetNodeBridgeLobbyClient<TNetwork>(this IEndpointRouteBuilder host)
            where TNetwork : BridgeLobbyBaseNetwork
            => host.ServiceProvider.GetRequiredService<TNetwork>();
    }
}
