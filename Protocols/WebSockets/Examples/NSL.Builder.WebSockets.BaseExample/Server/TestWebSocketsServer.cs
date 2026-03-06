using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.WebSockets.Server;

namespace Builder.WebSockets.BaseExample.Server
{
    internal class TestWebSocketsServer
    {
        public static Task RunServer()
        {
            var server = WebSocketsServerEndPointBuilder
                .Create()
                .WithClientProcessor<WebSocketsServerNetworkClient>()
                .WithOptions<WSServerOptions<WebSocketsServerNetworkClient>>()
                .WithBindingPoint("http://localhost:20006/")
                .WithCode(builder =>
                {
                    builder.WithBufferSize(8192); //optional
                })
                .WithCode(builder =>
                {
                    builder.AddConnectHandle(client =>
                    {
                        Console.WriteLine($"[Server] Success connected");
                    });

                    builder.AddDisconnectHandle(client =>
                    {
                        Console.WriteLine($"[Server] Client disconnected");
                    });

                    builder.AddExceptionHandle((ex, client) =>
                    {
                        Console.WriteLine($"[Server] Exception error handle - {ex}");
                    });

                    builder.AddSendHandle((client, pid, packet, stackTrace) =>
                    {
                        //Console.WriteLine($"[Server] Send packet({pid}) to {client.GetRemotePoint()} from\r\n{stackTrace}");
                        Console.WriteLine($"[Server] Send packet({pid}) to {client?.Network?.GetRemotePoint()}");
                    });

                    builder.AddReceiveHandle((client, pid, packet) =>
                    {
                        Console.WriteLine($"[Server] Receive packet({pid}) from {client?.Network?.GetRemotePoint()}");
                    });
                })
                .WithCode(builder =>
                {
                    builder.AddPacket(1, new ServerTestPacket1());
                })
                .Build();


            server.Start();

            return Task.CompletedTask;
        }
    }
}
