using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.AspNetCore;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils.Exceptions;

namespace NSL.Node.AspRoomServerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddNodeRoomServer();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseWebSockets();

            var roomServerLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RoomServer");
            var bridgeLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("BridgeClient");

            app.RunNodeRoomServer(c => c
            .WithAspLogger(bridgeLogger)
            .WithBridgeDefaultHandles()
            //.WithCreateSessionHandle(roomInfo=> new GameInfo(roomInfo))
            //.GetPublicAddressFromStun(out var publicAddr)
            .WithRoomBridgeNetwork("wss://localhost:7023/room_server", new Dictionary<string, string>(), "wss://localhost", NodeNetworkHandles<BridgeRoomNetworkClient>.Create()
                .WithExceptionHandle((ex, client) => {
                    if ((ex is ConnectionLostException cle && cle.InnerException == null)
                    || ex is TaskCanceledException)
                        return;

                    bridgeLogger?.LogError($"Client {client.Network?.GetRemotePoint()} have error - {ex.ToString()}");
                })
                .WithConnectHandle((client) => {

                    bridgeLogger.LogInformation($"Client {client.Network?.GetRemotePoint()} connected");

                    return Task.CompletedTask;
                })
                .WithDisconnectHandle((client) => {

                    bridgeLogger.LogInformation($"Client {client.Network?.GetRemotePoint()} disconnected");

                    return Task.CompletedTask;
                })
                .WithSendHandle((client, pid, len, stack) => {
                    bridgeLogger.LogInformation($"Send packet {pid} to {client.Network?.GetRemotePoint()}");
                })
                .WithReceiveHandle((client, pid, len) => {
                    bridgeLogger.LogInformation($"Receive packet {pid} from {client.Network?.GetRemotePoint()}");
                }))
            .WithClientServerAspBinding(app
            , "/room_server"
            , NodeNetworkHandles<TransportNetworkClient>.Create()
                .WithExceptionHandle((ex, client) => {
                    if ((ex is ConnectionLostException cle && cle.InnerException == null)
                    || ex is TaskCanceledException)
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
            ));

            app.Run();
        }
    }
}