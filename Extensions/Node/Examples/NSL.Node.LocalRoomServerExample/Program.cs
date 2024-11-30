using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.AspNetCore;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core;
using NSL.SocketCore.Utils.Exceptions;

namespace NSL.Node.LocalRoomServerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddNodeRoomServer();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseWebSockets();

            var roomServerLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RoomServer");

            app.RunNodeRoomServer(c => c
            .WithAspLogger(roomServerLogger)
            .WithHandleProcessor(new LocalAspRoomServerStartupEntry())
            //.WithCreateSessionHandle(roomInfo => new GameInfo(roomInfo))
            //.GetPublicAddressFromStun(out var publicAddr)
            .WithClientServerAspBinding(app
            , "/room_server"
            , NodeNetworkHandles< TransportNetworkClient>.Create()
                .WithExceptionHandle((ex, client) => {
                    if (ex is ConnectionLostException cle && cle.InnerException == null)
                        return;

                    roomServerLogger?.LogError($"Client {client.Network?.GetRemotePoint()} have error - {ex.ToString()}");
                })
                .WithConnectHandle((client) => {

                    roomServerLogger.LogInformation($"Client {client.Network?.GetRemotePoint()} connected");

                    return Task.CompletedTask;
                })
                .WithDisconnectHandle((client) => {

                    roomServerLogger.LogInformation($"Client {client.Network?.GetRemotePoint()} disconnected");

                    return Task.CompletedTask;
                })
                .WithSendHandle((client, pid, len, stack) => {
                    roomServerLogger.LogInformation($"Send packet {pid} to {client.Network?.GetRemotePoint()}");
                })
                .WithReceiveHandle((client, pid, len) => {
                    roomServerLogger.LogInformation($"Receive packet {pid} from {client.Network?.GetRemotePoint()}");
                })
            )
            );

            app.Run();
        }
    }
}