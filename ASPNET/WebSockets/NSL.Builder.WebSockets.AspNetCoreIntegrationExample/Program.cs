using Builder.WebSockets.AspNetCoreIntegrationExample.NetworkClient;
using Builder.WebSockets.AspNetCoreIntegrationExample.RPCContainers;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.Extensions.RPC.EndPointBuilder;
using NSL.SocketCore.Utils.Buffer;
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

            app.MapWebSocketsPoint("/server", builder =>
            {

                builder.AddConnectHandle(c =>
                {
                    app.Logger.LogInformation($"[Server] Client connected handle");
                    c.InitializeObjectBag();
                });
                builder.AddDisconnectHandle(c =>
                {
                    Console.WriteLine($"[Server] Client disconnected handle");
                });

                builder.AddReceiveHandle((client, pid, len) =>
                {
                    Console.WriteLine($"[Server] Receive packet pid:{pid} with len: {len}");
                });

                builder.AddSendHandle((client, pid, len, stack) =>
                {
                    Console.WriteLine($"[Server] Send packet pid:{pid} with len:{len}");
                });

                builder.AddExceptionHandle((ex, c) =>
                {
                    app.Logger.LogError($"[Server] Error {ex}");
                });

                builder.AddPacketHandle(1, (c, d) =>
                {
                    Console.WriteLine($"receive from client pid = {d.PacketId}, data = {d.ReadInt32()}");
                });

                //builder.RegisterRPCProcessor();
                //builder.AddRPCContainer(x => new TestRPCContainerRPCRepository<AspNetWSNetworkServerClient>());

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
                        Console.WriteLine($"[Client] Client connected handle");
                        connection = c;
                        c.InitializeObjectBag();
                        //c.PingPongEnabled = true;
                    });
                    builder.AddDisconnectHandle(c =>
                    {
                        Console.WriteLine($"[Client] Client disconnected handle");
                    });

                    builder.AddReceiveHandle((client, pid, len) =>
                    {
                        if (InputPacketBuffer.IsSystemPID(pid))
                            return;
                        Console.WriteLine($"[Client] Receive packet pid:{pid} with len: {len}");
                    });

                    builder.AddSendHandle((client, pid, len, stack) =>
                    {
                        Console.WriteLine($"[Client] Send packet pid:{pid} with len:{len}");
                    });

                    builder.AddExceptionHandle((ex, c) =>
                    {
                        app.Logger.LogError($"[Client] Error {ex}");
                    });

                    //builder.RegisterRPCProcessor();
                    //builder.AddRPCContainer(c => c.TestRepo);
                })
                .WithUrl(new Uri("wss://localhost:7084/server"))
                .Build();

            var result = await client.ConnectAsync();

            if (result)
            {
                for (int i = 0; i < 15; i++)
                {
                    Console.WriteLine($"send to server pid = {1}, data = {i}");
                    var packet = OutputPacketBuffer.Create(1);

                    packet.WriteInt32(i);

                    client.Send(packet);
                }

                //Console.WriteLine($"[Client] Try call \"test\" method with 22 value");

                //var r = connection.TestRepo.test(22);

                //Console.WriteLine($"[Client] Try call \"test\" with 22 return {r} value");

                //Console.WriteLine($"[Client] Try call \"test\" method with 33 value");

                //r = connection.TestRepo.test(33);

                //Console.WriteLine($"[Client] Try call \"test\" with 33 return {r} value");

                //Console.WriteLine($"[Client] Try call \"test\" method with 44 value");

                //r = connection.TestRepo.test(44);

                //Console.WriteLine($"[Client] Try call \"test\" with 44 return {r} value");

                //Console.WriteLine($"[Client] Try call \"testasynctaskwithresult\" with 66");

                //r = await connection.TestRepo.testasynctaskwithresult(66);

                //Console.WriteLine($"[Client] Try call \"testasynctaskwithresult\" with 66 return {r} value");

                //Console.WriteLine($"[Client] Try call \"testasyncvoid\" with 122");

                //connection.TestRepo.testasyncvoid(122);

                //Console.WriteLine($"[Client] Try call \"testasyncvoid\" has not result");

                //Console.WriteLine($"[Client] Try call \"testasyncTask\" with 244");

                //await connection.TestRepo.testasyncTask(244);

                //Console.WriteLine($"[Client] Try call \"testasyncTask\" has not result");
            }

            await Task.Delay(2000);
        }
    }
}