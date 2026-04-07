using Microsoft.Extensions.DependencyInjection;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.TCPServer;

namespace NSL.Builder.TCP.BaseExample.Server
{
    internal class TestTcpServer
    {
        public static Task RunServer()
        {
            var server = TCPServerEndPointBuilder
                .Create()
                .WithClientProcessor<TCPServerNetworkClient>()
                .WithOptions()
                .WithBindingPoint("0.0.0.0", 20006)
                .WithBacklog(1)
                .WithCode(builder =>
                {
                    builder.WithBufferSize(8192); //optional
                })
                .WithCode(builder =>
                {
                    // Регистрация DI-сервисов через extension из BuilderExtensions.SocketCore:
                    // - Singleton живёт на весь срок работы сервера
                    // - Scoped создаётся на каждого клиента после InitializeServiceScope
                    builder.WithServices(services =>
                    {
                        services.AddSingleton<ServerStats>();
                        services.AddScoped<ClientSession>();
                    });
                })
                .WithCode(builder =>
                {
                    builder.AddConnectHandle(client =>
                    {
                        // Scope НЕ инициализируется на connect — только после авторизации.
                        // Singleton доступен напрямую через options.ServiceProvider.
                        Console.WriteLine($"[Server] Client connected from {client.Network?.GetRemotePoint()}");
                    });

                    builder.AddDisconnectHandle(client =>
                    {
                        // Scope (если был) освобождается автоматически в Dispose клиента.
                        var sessionInfo = client.ServiceScopeInitialized()
                            ? $"UserId={client.ServiceScope.ServiceProvider.GetRequiredService<ClientSession>().UserId}"
                            : "not authorized";
                        Console.WriteLine($"[Server] Client disconnected ({sessionInfo})");
                    });

                    builder.AddExceptionHandle((ex, client) =>
                    {
                        Console.WriteLine($"[Server] Exception error handle - {ex}");
                    });
                })
                .WithCode(builder =>
                {
                    builder.AddPacket(1, new ServerTestPacket1());

                    // Пример авторизационного пакета (packet 2):
                    // builder.AddPacketHandle(2, (client, data) =>
                    // {
                    //     var userId = data.ReadInt32();
                    //     // InitializeServiceScope — thread-safe, повторный вызов игнорируется
                    //     if (client.InitializeServiceScope(builder.GetCoreOptions().ServiceProvider))
                    //     {
                    //         var session = client.ServiceScope.ServiceProvider.GetRequiredService<ClientSession>();
                    //         session.UserId = userId;
                    //     }
                    // });
                })
                .Build();

            server.Start();

            return Task.CompletedTask;
        }
    }

    /// <summary>Singleton: общая статистика сервера.</summary>
    class ServerStats
    {
        private int _connections;
        public int Connections => _connections;
        public void OnConnect() => Interlocked.Increment(ref _connections);
        public void OnDisconnect() => Interlocked.Decrement(ref _connections);
    }

    /// <summary>Scoped: сессия конкретного клиента, создаётся после авторизации.</summary>
    class ClientSession
    {
        public int UserId { get; set; }
    }
}
