using Microsoft.Extensions.DependencyInjection;
using NSL.Cipher.RC.RC4;
using NSL.Logger;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.TCP.Server;

Console.WriteLine("TCP.Server");

// ---- DI: регистрация singleton-сервисов уровня сервера ----
var services = new ServiceCollection();
services.AddSingleton<ConnectionCounter>();
// Scoped-сервисы создаются на каждого клиента после инициализации scope:
services.AddScoped<ClientSession>();

ServerOptions<BaseNetworkConnection> options = new ServerOptions<BaseNetworkConnection>();
options.ServiceProvider = services.BuildServiceProvider();

options.WithBindingEndPoint("0.0.0.0", 20008);

options.ReceiveBufferSize = 1024;

options.HelperLogger = new ConsoleLogger();

options.InputCipher = new XRC4Cipher("werty65343g353g");
options.OutputCipher = new XRC4Cipher("werty65343g353g");

options.OnExceptionEvent += (ex, c) =>
{
    Console.WriteLine($"Exception {ex}");
};

// Packet 1 — доступен всем (до авторизации)
options.AddHandle(1, (client, p) =>
{
    Console.WriteLine($"Receive from client {p.ReadString()}");

    var o = OutputPacketBuffer.Create(4);
    o.WriteInt32(p.DataLength);
    client.Send(o);
});

// Packet 2 — "авторизация": инициализирует scope и получает scoped-сервисы
options.AddHandle(2, (client, p) =>
{
    var userId = p.ReadInt32();

    // Scope создаётся один раз; повторные вызовы игнорируются (thread-safe)
    if (client.InitializeServiceScope(options.ServiceProvider))
    {
        var session = client.ServiceScope.ServiceProvider.GetRequiredService<ClientSession>();
        session.UserId = userId;
        Console.WriteLine($"Client authorized, UserId={session.UserId}");
    }
});

// Packet 3 — требует авторизованного scope
options.AddHandle(3, (client, p) =>
{
    if (!client.ServiceScopeInitialized())
    {
        client.Network?.Disconnect();
        return;
    }

    var session = client.ServiceScope.ServiceProvider.GetRequiredService<ClientSession>();
    Console.WriteLine($"Authorized request from UserId={session.UserId}");
});

options.AddHandle(7, (c, req) =>
{
    var d = req.ReadNullableClass<object>(() =>
    {
        var s1 = Enumerable.Range(0, 1000).Select(x => req.ReadString()).ToArray();
        return s1;
    });
});

// Connect: scope НЕ инициализируется — ждём авторизацию
options.OnClientConnectEvent += (client) =>
{
    var counter = options.ServiceProvider.GetRequiredService<ConnectionCounter>();
    counter.Increment();

    Console.WriteLine($"Client connected (total: {counter.Total})");

    var outputPacketBuffer = new OutputPacketBuffer();
    outputPacketBuffer.PacketId = 1;
    outputPacketBuffer.WriteString("Hello! I'm server");
    client.Send(outputPacketBuffer);
};

// Disconnect: scope освобождается автоматически в Dispose клиента
options.OnClientDisconnectEvent += (client) =>
{
    var sessionInfo = client.ServiceScopeInitialized()
        ? $"UserId={client.ServiceScope.ServiceProvider.GetRequiredService<ClientSession>().UserId}"
        : "not authorized";
    Console.WriteLine($"Client disconnected ({sessionInfo})");
};

var t = new TCPServerListener<BaseNetworkConnection>(options, false);

t.Start();

Thread.Sleep(Timeout.Infinite);

// ---- Вспомогательные типы ----

/// <summary>Singleton уровня сервера — общий счётчик подключений.</summary>
class ConnectionCounter
{
    private int _total;
    public int Total => _total;
    public void Increment() => Interlocked.Increment(ref _total);
}

/// <summary>Scoped уровня клиента — создаётся после авторизации.</summary>
class ClientSession
{
    public int UserId { get; set; }
}
