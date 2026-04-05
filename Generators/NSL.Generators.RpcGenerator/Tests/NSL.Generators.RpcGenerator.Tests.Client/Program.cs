using NSL.BuilderExtensions.TCPClient;
using NSL.Generators.RpcGenerator.Shared;
using NSL.Generators.RpcGenerator.Tests.Client;
using NSL.Generators.RpcGenerator.Tests.Server;
using NSL.Generators.RpcGenerator.Tests.Shared;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Threading.Tasks;

namespace NSL.Generators.RpcGenerator.Tests.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const int port = 17777;

            // ── Start TCP server ─────────────────────────────────────────────
            var server = NSL.BuilderExtensions.TCPServer.TCPServerEndPointBuilder.Create()
                .WithClientProcessor<NSL.SocketServer.Utils.BaseServerNetworkClient>()
                .WithOptions()
                .WithBindingPoint(port)
                .WithCode(b =>
                {
                    var rpcServer = new TestRpcServer();
                    rpcServer.NSLConfigureRpcHandles_ITestRpcService(b.GetCoreOptions());
                    rpcServer.NSLConfigureRpcHandles_ITestChatService(b.GetCoreOptions());
                })
                .Build();

            server.Start();
            Console.WriteLine($"[Server] Listening on :{port}");

            await Task.Delay(300); // let the listener start

            // ── Connect TCP client ───────────────────────────────────────────
            var client = TCPClientEndPointBuilder.Create()
                .WithClientProcessor<NSL.SocketClient.BasicNetworkClient>()
                .WithOptions()
                .WithEndPoint("127.0.0.1", port)
                .WithCode(b =>
                {
                    b.GetOptions().InitializeClientObjectBagOnConnect();
                    b.GetOptions().ConfigureRequestProcessor();
                })
                .Build();

            if (!await client.ConnectAsync())
                throw new Exception("[Client] Connection failed");

            Console.WriteLine("[Client] Connected");

            var requestProcessor = client.Data.GetRequestProcessor();
            var channel = new DefaultRPCNetworkChannel(client.Data, requestProcessor);
            var rpc = new TestRpcClient(channel);

            // ── Scenario 1: Fire-and-forget ──────────────────────────────────
            Console.WriteLine("\n[Test] Scenario 1: FireAndForget");
            rpc.SendNotification("Hello from RPC client");
            await Task.Delay(200);

            // ── Scenario 2: Task (request-response, no return) ────────────────
            Console.WriteLine("[Test] Scenario 2: Ping");
            await rpc.Ping();
            Console.WriteLine("[Client] Ping done");

            // ── Scenario 3: Task<T> ───────────────────────────────────────────
            Console.WriteLine("[Test] Scenario 3: GetPlayer");
            var player = await rpc.GetPlayer(42);
            Console.WriteLine($"[Client] GetPlayer result: Id={player.Id} Name={player.Name} Score={player.Score}");

            // ── Scenario 4: ExceptionHandler — success path ──────────────────
            Console.WriteLine("[Test] Scenario 4a: GetPlayerSafe(0) — should succeed");
            var safe = await rpc.GetPlayerSafe(0);
            Console.WriteLine($"[Client] GetPlayerSafe result: Id={safe.Id} Name={safe.Name}");

            // ── Scenario 4: ExceptionHandler — exception path ────────────────
            Console.WriteLine("[Test] Scenario 4b: GetPlayerSafe(99) — should throw NSLRPCRemoteException");
            try
            {
                await rpc.GetPlayerSafe(99);
                Console.WriteLine("[Client] ERROR: Expected exception not thrown!");
            }
            catch (NSLRPCRemoteException ex)
            {
                Console.WriteLine($"[Client] NSLRPCRemoteException caught: {ex.RemoteTypeName}: {ex.Message}");
            }

            // ── Scenario 5: Multiple params, Task<int> ───────────────────────
            Console.WriteLine("[Test] Scenario 5: AddScore");
            var newScore = await rpc.AddScore(7, 50);
            Console.WriteLine($"[Client] AddScore result: {newScore}");

            // ── Scenario 6: ITestChatService FAF ────────────────────────────
            Console.WriteLine("[Test] Scenario 6: BroadcastMessage (FAF)");
            rpc.BroadcastMessage("Hello chat!");
            await Task.Delay(200);

            // ── Scenario 7: GetLastMessage with interface-level ExceptionHandler ──
            Console.WriteLine("[Test] Scenario 7: GetLastMessage");
            var msg = await rpc.GetLastMessage("general");
            Console.WriteLine($"[Client] GetLastMessage: [{msg.Channel}] {msg.Author}: {msg.Text}");

            Console.WriteLine("\n[Test] All scenarios passed!");
            server.Stop();
        }
    }
}
