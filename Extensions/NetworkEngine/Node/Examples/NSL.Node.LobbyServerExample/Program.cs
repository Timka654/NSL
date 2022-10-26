using Microsoft.Extensions.DependencyInjection;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.Node.BridgeLobbyClient;
using NSL.Node.LobbyServerExample.Managers;
using NSL.Node.LobbyServerExample.Shared.Models;

namespace NSL.Node.LobbyServerExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<LobbyManager>();
            builder.Services.AddSingleton<BridgeLobbyNetwork>(services => new BridgeLobbyNetwork(
                new Uri(builder.Configuration.GetValue("bridge:server:url", "ws://localhost:6999")),
                builder.Configuration.GetValue("bridge:server:identity", "270E1B1E-4889-4D46-8B9D-9325404FFD69"),
                builder.Configuration.GetValue("bridge:server:key", "270E1B1E-4889-4D46-8B9D-9325404FFD69"),
                builder =>
                {
                })
            {
                ValidateSession = services.GetRequiredService<LobbyManager>().BridgeValidateSessionAsync
            });

            var app = builder.Build();


            app.Services.GetRequiredService<BridgeLobbyNetwork>().Initialize();


            app.UseWebSockets();

            app.MapWebSocketsPoint<LobbyNetworkClientModel>("/lobby_ws", builder =>
            {
                app.Services.GetRequiredService<LobbyManager>().BuildNetwork(builder);
            });

            app.UseRouting();

            app.Run();
        }
    }
}