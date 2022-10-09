using Builder.WebSockets.AspNetCoreIntegrationExample.NetworkClient;
using Builder.WebSockets.AspNetCoreIntegrationExample.RPCContainers;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSockets;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.Extensions.RPC.EndPointBuilder;
using NSL.SocketClient;
using NSL.WebSockets.Client;
using NSL.WebSockets.Server.AspNetPoint;

namespace Builder.WebSockets.AspNetCoreIntegrationExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();

            app.UseWebSockets();

            app.MapWebSocketsPoint("/server", builder => {

                builder.AddConnectHandle(c =>
                {
                    app.Logger.LogInformation($"[Server] Client connected handle");
                    c.InitializeObjectBag();
                });
                builder.AddDisconnectHandle(c =>
                {
                    app.Logger.LogInformation($"[Server] Client disconnected handle");
                });

                builder.AddReceiveHandle((client, pid, len) => {
                    app.Logger.LogInformation($"[Server] Receive packet pid:{pid} with len: {len}");
                });

                builder.AddSendHandle((client, pid, len, stack) => {
                    app.Logger.LogInformation($"[Server] Send packet pid:{pid} with len:{len}");
                });

                builder.AddExceptionHandle((ex, c) =>
                {
                    app.Logger.LogError($"[Server] Error {ex}");
                });

                builder.RegisterRPCProcessor();
                builder.AddRPCContainer(x => new TestRPCContainerRPCRepository<AspNetWSNetworkServerClient>());

            });

            app.MapGet("/", () => "Hello World!");

            testClient(app);

            app.Run();
        }

        private static async void testClient(WebApplication app)
        {
            await Task.Delay(2000);

            WSNetworkClientInfo connection = default;

            var client = WebSocketsClientEndPointBuilder
                .Create()
                .WithClientProcessor<WSNetworkClientInfo>()
                .WithOptions<WSClientOptions<WSNetworkClientInfo>>()
                .WithCode(builder =>
                {
                    builder.AddConnectHandle(c =>
                    {
                        app.Logger.LogInformation($"[Client] Client connected handle");
                        connection = c;
                        c.InitializeObjectBag();
                    });
                    builder.AddDisconnectHandle(c =>
                    {
                        app.Logger.LogInformation($"[Client] Client disconnected handle");
                    });

                    builder.AddReceiveHandle((client, pid, len) => {
                        app.Logger.LogInformation($"[Client] Receive packet pid:{pid} with len: {len}");
                    });

                    builder.AddSendHandle((client, pid, len, stack) => {
                        app.Logger.LogInformation($"[Client] Send packet pid:{pid} with len:{len}");
                    });

                    builder.AddExceptionHandle((ex, c) => {
                        app.Logger.LogError($"[Client] Error {ex}");
                    });

                    builder.RegisterRPCProcessor();
                    builder.AddRPCContainer(c => c.TestRepo);
                })
                .WithUrl(new Uri("wss://localhost:7084/server"))
                .Build();

            var result = await client.ConnectAsync();

            if (result)
            {
                app.Logger.LogInformation($"[Client] Try call \"test\" method with 22 value");

                var r = connection.TestRepo.test(22);

                app.Logger.LogInformation($"[Client] Try call \"test\" with 22 return {r} value");

                app.Logger.LogInformation($"[Client] Try call \"test\" method with 33 value");

                r = connection.TestRepo.test(33);

                app.Logger.LogInformation($"[Client] Try call \"test\" with 33 return {r} value");

                app.Logger.LogInformation($"[Client] Try call \"test\" method with 44 value");

                r = connection.TestRepo.test(44);

                app.Logger.LogInformation($"[Client] Try call \"test\" with 44 return {r} value");
            }

            await Task.Delay(2000);
        }
    }
}