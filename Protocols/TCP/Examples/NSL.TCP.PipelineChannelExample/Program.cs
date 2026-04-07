// ──────────────────────────────────────────────────────────────────────────────
// NSL Pipeline Channel Example
//
// Demonstrates four receive-channel configurations using middleware:
//
//  pid=10  Plain      — no middleware; backward-compat AddHandle terminal
//  pid=11  CRC        — CRC-16 header validation
//  pid=12  Logging    — per-packet log of receive/send events
//  pid=13  Full       — CRC + Logging + PacketHandleRouter (all three combined)
//
// Server and client both run in the same process for a self-contained demo.
// ──────────────────────────────────────────────────────────────────────────────

using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Pipeline.Middleware;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.TCP.Client;
using NSL.TCP.Server;

const int Port      = 20099;
const int PidPlain  = 10;
const int PidCrc    = 11;
const int PidLog    = 12;
const int PidFull   = 13;

// ── Helpers ──────────────────────────────────────────────────────────────────

static void Log(string tag, string msg)
    => Console.WriteLine($"[{tag,6}] {msg}");

// ── Server setup ─────────────────────────────────────────────────────────────

var serverOptions = new ServerOptions<BaseNetworkConnection>();
serverOptions.WithBindingEndPoint("0.0.0.0", Port);
serverOptions.OnExceptionEvent += (ex, _) => Log("SRV ERR", ex.Message);

// pid=10: plain backward-compat — uses AddHandle; pipeline terminal is auto-registered
serverOptions.AddHandle(PidPlain, (conn, buf) =>
{
    var msg = buf.ReadString();
    Log("SRV 10", $"Plain received: '{msg}'");

    var reply = OutputPacketBuffer.Create(PidPlain);
    reply.WriteString($"echo:{msg}");
    conn.Send(reply);
});

// pid=11: CRC validation — middleware validates checksum, router handles packet
var crcMw = new CrcChannelMiddleware();

var router11 = new PacketHandleRouterMiddleware()
    .AddRoute(PidCrc, (conn, buf) =>
    {
        var msg = buf.ReadString();
        Log("SRV 11", $"CRC-validated received: '{msg}'");

        // Reply via pipeline (sends with CRC in channel header)
        var body = new PacketBodyBuffer();
        body.WriteString($"crc-echo:{msg}");
        _ = conn.GetChannel(PidCrc).SendAsync(conn, body);
    });

serverOptions.GetOrCreateChannel(PidCrc)
    .UseReceive(crcMw)
    .UseReceive(router11)
    .UseSend(crcMw);

// pid=12: logging only
var logMw = new LoggingChannelMiddleware(m => Log("LOG MW", m));

var router12 = new PacketHandleRouterMiddleware()
    .AddRoute(PidLog, ctx =>
    {
        // Use CreateReader() for familiar InputPacketBuffer-style reading
        using var buf = ctx.CreateReader();
        var msg = buf.ReadString();
        Log("SRV 12", $"Logged received: '{msg}'");

        var body = new PacketBodyBuffer();
        body.WriteString($"log-echo:{msg}");
        _ = ctx.Connection.GetChannel(PidLog).SendAsync(ctx.Connection, body);
    });

serverOptions.GetOrCreateChannel(PidLog)
    .UseReceive(logMw)
    .UseReceive(router12)
    .UseSend(logMw);

// pid=13: CRC + Logging + Router — full stack
var crcMw13 = new CrcChannelMiddleware();
var logMw13 = new LoggingChannelMiddleware(m => Log("FULL", m));

var router13 = new PacketHandleRouterMiddleware()
    .AddRoute(PidFull, ctx =>
    {
        using var buf = ctx.CreateReader();
        var msg = buf.ReadString();
        Log("SRV 13", $"Full-stack received: '{msg}'");

        var body = new PacketBodyBuffer();
        body.WriteString($"full-echo:{msg}");
        _ = ctx.Connection.GetChannel(PidFull).SendAsync(ctx.Connection, body);
    });

serverOptions.GetOrCreateChannel(PidFull)
    .UseReceive(crcMw13)
    .UseReceive(logMw13)
    .UseReceive(router13)
    .UseSend(crcMw13)
    .UseSend(logMw13);

var server = new TCPServerListener<BaseNetworkConnection>(serverOptions);
server.Start();
Log("SRV", $"Listening on :{Port}");

// ── Client setup ─────────────────────────────────────────────────────────────

await Task.Delay(200); // wait for server to bind

var clientOptions = new ClientOptions<BaseNetworkConnection>();
clientOptions.OnExceptionEvent += (ex, _) => Log("CLI ERR", ex.Message);

// pid=10: plain — AddHandle; no channel headers
clientOptions.AddHandle(PidPlain, (conn, buf) =>
    Log("CLI 10", $"Got reply: '{buf.ReadString()}'"));

// pid=11: CRC — send + receive with checksum
var cliCrcMw = new CrcChannelMiddleware();

var cliRouter11 = new PacketHandleRouterMiddleware()
    .AddRoute(PidCrc, (conn, buf) =>
        Log("CLI 11", $"Got crc-reply: '{buf.ReadString()}'"));

clientOptions.GetOrCreateChannel(PidCrc)
    .UseReceive(cliCrcMw)
    .UseReceive(cliRouter11)
    .UseSend(cliCrcMw);

// pid=12: logging
var cliLogMw = new LoggingChannelMiddleware(m => Log("CLI LOG", m));

var cliRouter12 = new PacketHandleRouterMiddleware()
    .AddRoute(PidLog, (conn, buf) =>
        Log("CLI 12", $"Got log-reply: '{buf.ReadString()}'"));

clientOptions.GetOrCreateChannel(PidLog)
    .UseReceive(cliLogMw)
    .UseReceive(cliRouter12)
    .UseSend(cliLogMw);

// pid=13: full stack
var cliCrcMw13 = new CrcChannelMiddleware();
var cliLogMw13 = new LoggingChannelMiddleware(m => Log("CLI FL", m));

var cliRouter13 = new PacketHandleRouterMiddleware()
    .AddRoute(PidFull, (conn, buf) =>
        Log("CLI 13", $"Got full-reply: '{buf.ReadString()}'"));

clientOptions.GetOrCreateChannel(PidFull)
    .UseReceive(cliCrcMw13)
    .UseReceive(cliLogMw13)
    .UseReceive(cliRouter13)
    .UseSend(cliCrcMw13)
    .UseSend(cliLogMw13);

var client = new TCPNetworkClient<BaseNetworkConnection>(clientOptions);
if (!client.Connect("127.0.0.1", Port))
{
    Log("CLI", "Connection failed");
    return;
}
Log("CLI", "Connected");
await Task.Delay(100);

var conn = clientOptions.ClientData;

// ── Send packets through each channel variant ────────────────────────────────

// pid=10: classic OutputPacketBuffer send (no pipeline channel headers)
var p10 = OutputPacketBuffer.Create(PidPlain);
p10.WriteString("hello-plain");
conn.Send(p10);
await Task.Delay(150);

// pid=11: pipeline send (CRC written into channel header)
var body11 = new PacketBodyBuffer();
body11.WriteString("hello-crc");
await conn.GetChannel(PidCrc).SendAsync(conn, body11);
await Task.Delay(150);

// pid=12: pipeline send (logged)
var body12 = new PacketBodyBuffer();
body12.WriteString("hello-log");
await conn.GetChannel(PidLog).SendAsync(conn, body12);
await Task.Delay(150);

// pid=13: pipeline send (CRC + logged)
var body13 = new PacketBodyBuffer();
body13.WriteString("hello-full");
await conn.GetChannel(PidFull).SendAsync(conn, body13);
await Task.Delay(300);

Log("DONE", "All packets sent and received");
client.Disconnect();
server.Stop();
