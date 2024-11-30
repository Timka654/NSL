
using NSL.LocalBridge;
using NSL.Node.BridgeLobbyClient;
using NSL.Node.BridgeLobbyClient.AspNetCore;
using NSL.Node.BridgeServer;
using NSL.Node.BridgeServer.LS;

namespace NSL.Node.AspNetExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            AddNode(builder.Services);

            var app = builder.Build();

            app.UseWebSockets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            RunNode(app, app.Logger);

            app.Run();
        }

        static IServiceCollection AddNode(IServiceCollection services)
        {
            services.AddNodeBridgeServer();

            services.AddNodeBridgeLobbyLocalBridgeClient<LobbyServerNetworkClient>(
                string.Empty,
                string.Empty,
                (services, handles) => {

                });

            return services;
        }

        static IEndpointRouteBuilder RunNode(IEndpointRouteBuilder builder, ILogger logger)
        {
            LocalBridgeClient<LobbyServerNetworkClient, BridgeLobbyNetworkClient> nodeLocalLobbyServerClient = default;

            builder.RunNodeBridgeServer(c => c
                .WithAspLogger(logger)
                .WithDefaultManagers(string.Empty)
                .WithRoomServerAspBinding(builder, "/room_server")
                .WithLobbyServerLocalBridgeBinding(out nodeLocalLobbyServerClient));

            builder.RunNodeBridgeLobbyLocalBridgeClient(nodeLocalLobbyServerClient);

            return builder;

        }
    }
}